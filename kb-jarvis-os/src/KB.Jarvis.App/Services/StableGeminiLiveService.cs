using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using NAudio.Wave;

namespace KB.Jarvis.App.Services;

public sealed class StableGeminiLiveService : IAsyncDisposable
{
    private sealed record VisualPacket(byte[] Jpeg, string Source);

    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly Channel<byte[]> _audioQueue = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.Wait
    });
    private readonly Channel<VisualPacket> _visualQueue = Channel.CreateBounded<VisualPacket>(new BoundedChannelOptions(1)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.DropOldest
    });

    private ClientWebSocket? _socket;
    private CancellationTokenSource? _sessionCts;
    private WaveInEvent? _microphone;
    private BufferedWaveProvider? _playbackBuffer;
    private WaveOutEvent? _speaker;
    private Task? _receiveTask;
    private Task? _mediaSendTask;
    private TaskCompletionSource<bool>? _setupCompletion;
    private string? _sessionHandle;
    private long _speakerActiveUntilTicks;
    private long _suppressedMicPackets;
    private long _droppedMicPackets;
    private long _playbackUnderruns;
    private long _trimmedPlaybackBytes;
    private int _queuedAudioPackets;
    private bool _playbackStarted;

    public event Action<string>? StateChanged;
    public event Action<string>? InputTranscript;
    public event Action<string>? OutputTranscript;
    public event Action<string>? Diagnostic;

    public Func<string, JsonElement, CancellationToken, Task<string>>? ToolExecutor { get; set; }
    public Func<string>? ContextProvider { get; set; }
    public bool IsRunning => _socket?.State == WebSocketState.Open && _microphone is not null;

    public async Task StartAsync(JarvisSettings settings, CancellationToken cancellationToken)
    {
        if (IsRunning) return;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is missing. Open Jarvis Settings first.");
        }

        await StopAsync().ConfigureAwait(false);
        DrainMediaQueues();
        Interlocked.Exchange(ref _speakerActiveUntilTicks, 0);
        Interlocked.Exchange(ref _suppressedMicPackets, 0);
        Interlocked.Exchange(ref _droppedMicPackets, 0);
        Interlocked.Exchange(ref _playbackUnderruns, 0);
        Interlocked.Exchange(ref _trimmedPlaybackBytes, 0);
        Interlocked.Exchange(ref _queuedAudioPackets, 0);
        _playbackStarted = false;

        _sessionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = _sessionCts.Token;
        _setupCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _socket = new ClientWebSocket();
        _socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

        var endpoint = new Uri(
            "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent" +
            $"?key={Uri.EscapeDataString(settings.ApiKey)}");
        StateChanged?.Invoke(string.IsNullOrWhiteSpace(_sessionHandle) ? "CONNECTING" : "RESUMING");
        await _socket.ConnectAsync(endpoint, token).ConfigureAwait(false);
        await SendSetupAsync(settings, token).ConfigureAwait(false);
        _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);

        using var setupTimeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        setupTimeout.CancelAfter(TimeSpan.FromSeconds(15));
        await _setupCompletion.Task.WaitAsync(setupTimeout.Token).ConfigureAwait(false);

        StartAudioDevices();
        _mediaSendTask = Task.Run(() => MediaSendLoopAsync(token), token);
        SpeechModeCoordinator.SetLiveVoiceActive(true);
        StateChanged?.Invoke("LISTENING");
        Diagnostic?.Invoke(
            "Stable Live voice ready: adaptive prebuffering, bounded microphone continuity, echo suppression, one-engine speech arbitration and session resumption are active.");
    }

    public async Task StopAsync()
    {
        SpeechModeCoordinator.SetLiveVoiceActive(false);
        var session = _sessionCts;
        _sessionCts = null;
        try { session?.Cancel(); } catch { }

        if (_microphone is not null)
        {
            try { _microphone.StopRecording(); } catch { }
            _microphone.DataAvailable -= MicrophoneOnDataAvailable;
            _microphone.Dispose();
            _microphone = null;
        }

        if (_speaker is not null)
        {
            try { _speaker.Stop(); } catch { }
            _speaker.Dispose();
            _speaker = null;
            _playbackBuffer = null;
            _playbackStarted = false;
        }

        if (_mediaSendTask is not null)
        {
            try { await _mediaSendTask.ConfigureAwait(false); } catch { }
            _mediaSendTask = null;
        }

        var socket = _socket;
        _socket = null;
        if (socket is not null)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Jarvis stable voice stopped",
                            CancellationToken.None)
                        .ConfigureAwait(false);
                }
            }
            catch { }
            socket.Dispose();
        }

        if (_receiveTask is not null)
        {
            try { await _receiveTask.ConfigureAwait(false); } catch { }
            _receiveTask = null;
        }

        DrainMediaQueues();
        session?.Dispose();
        StateChanged?.Invoke("OFFLINE");
    }

    public Task SendVideoFrameAsync(byte[] jpeg, string source, CancellationToken cancellationToken)
    {
        if (jpeg.Length == 0 || _socket?.State != WebSocketState.Open || cancellationToken.IsCancellationRequested)
            return Task.CompletedTask;
        _visualQueue.Writer.TryWrite(new VisualPacket(jpeg, source));
        return Task.CompletedTask;
    }

    private async Task SendSetupAsync(JarvisSettings settings, CancellationToken cancellationToken)
    {
        var model = settings.LiveModel.StartsWith("models/", StringComparison.OrdinalIgnoreCase)
            ? settings.LiveModel
            : $"models/{settings.LiveModel}";
        var context = ContextProvider?.Invoke();
        var instruction = string.IsNullOrWhiteSpace(context)
            ? OperationalSystemInstruction.Text
            : OperationalSystemInstruction.Text +
              "\n\nCURRENT PERSISTED OPERATIONAL CONTEXT\n" + context;

        await SendJsonAsync(new
        {
            setup = new
            {
                model,
                generationConfig = new
                {
                    responseModalities = new[] { "AUDIO" },
                    speechConfig = new
                    {
                        voiceConfig = new
                        {
                            prebuiltVoiceConfig = new { voiceName = settings.VoiceName }
                        }
                    }
                },
                systemInstruction = new
                {
                    parts = new[] { new { text = instruction } }
                },
                tools = new[]
                {
                    new { functionDeclarations = GeminiToolCatalog.CreateFunctionDeclarations() }
                },
                inputAudioTranscription = new { },
                outputAudioTranscription = new { },
                contextWindowCompression = new { slidingWindow = new { } },
                sessionResumption = new { handle = _sessionHandle }
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private void StartAudioDevices()
    {
        _playbackBuffer = new BufferedWaveProvider(new WaveFormat(24000, 16, 1))
        {
            BufferDuration = TimeSpan.FromSeconds(6),
            DiscardOnBufferOverflow = false,
            ReadFully = true
        };
        _speaker = new WaveOutEvent
        {
            DesiredLatency = 120,
            NumberOfBuffers = 3
        };
        _speaker.Init(_playbackBuffer);

        _microphone = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 40,
            NumberOfBuffers = 6
        };
        _microphone.DataAvailable += MicrophoneOnDataAvailable;
        _microphone.StartRecording();
    }

    private void MicrophoneOnDataAvailable(object? sender, WaveInEventArgs eventArgs)
    {
        if (_socket?.State != WebSocketState.Open || _sessionCts?.IsCancellationRequested != false) return;
        if (Environment.TickCount64 < Interlocked.Read(ref _speakerActiveUntilTicks))
        {
            Interlocked.Increment(ref _suppressedMicPackets);
            return;
        }

        var copy = new byte[eventArgs.BytesRecorded];
        Buffer.BlockCopy(eventArgs.Buffer, 0, copy, 0, copy.Length);
        if (_audioQueue.Writer.TryWrite(copy))
        {
            Interlocked.Increment(ref _queuedAudioPackets);
            return;
        }

        if (_audioQueue.Reader.TryRead(out _))
        {
            Interlocked.Decrement(ref _queuedAudioPackets);
            Interlocked.Increment(ref _droppedMicPackets);
        }
        if (_audioQueue.Writer.TryWrite(copy))
        {
            Interlocked.Increment(ref _queuedAudioPackets);
        }
    }

    private async Task MediaSendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workDone = false;
                var audioBurst = 0;
                while (audioBurst < 12 && _audioQueue.Reader.TryRead(out var audio))
                {
                    Interlocked.Decrement(ref _queuedAudioPackets);
                    await SendAudioPacketAsync(audio, cancellationToken).ConfigureAwait(false);
                    audioBurst++;
                    workDone = true;
                }

                if (_visualQueue.Reader.TryRead(out var visual))
                {
                    await SendVisualPacketAsync(visual, cancellationToken).ConfigureAwait(false);
                    workDone = true;
                }

                if (workDone) continue;
                var audioWait = _audioQueue.Reader.WaitToReadAsync(cancellationToken).AsTask();
                var visualWait = _visualQueue.Reader.WaitToReadAsync(cancellationToken).AsTask();
                await Task.WhenAny(audioWait, visualWait).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            Diagnostic?.Invoke($"Stable media send loop error: {exception.Message}");
        }
    }

    private Task SendAudioPacketAsync(byte[] audio, CancellationToken cancellationToken) =>
        SendJsonAsync(new
        {
            realtimeInput = new
            {
                audio = new
                {
                    mimeType = "audio/pcm;rate=16000",
                    data = Convert.ToBase64String(audio)
                }
            }
        }, cancellationToken);

    private async Task SendVisualPacketAsync(VisualPacket packet, CancellationToken cancellationToken)
    {
        try
        {
            await SendJsonAsync(new
            {
                realtimeInput = new
                {
                    video = new
                    {
                        mimeType = "image/jpeg",
                        data = Convert.ToBase64String(packet.Jpeg)
                    }
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            Diagnostic?.Invoke($"{packet.Source} visual stream error: {exception.Message}");
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_socket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var json = await ReceiveTextMessageAsync(_socket, cancellationToken).ConfigureAwait(false);
                if (json is null) break;
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("setupComplete", out _))
                {
                    _setupCompletion?.TrySetResult(true);
                    StateChanged?.Invoke("CONNECTED");
                }

                if (root.TryGetProperty("sessionResumptionUpdate", out var resumptionUpdate))
                {
                    var resumable = resumptionUpdate.TryGetProperty("resumable", out var resumableElement)
                                    && resumableElement.ValueKind == JsonValueKind.True;
                    var handle = resumptionUpdate.TryGetProperty("newHandle", out var handleElement)
                        ? handleElement.GetString()
                        : null;
                    if (resumable && !string.IsNullOrWhiteSpace(handle))
                    {
                        _sessionHandle = handle;
                        Diagnostic?.Invoke("Stable Gemini Live session resumption handle refreshed.");
                    }
                }

                if (root.TryGetProperty("serverContent", out var serverContent))
                {
                    ReadTranscriptions(serverContent);
                    if (serverContent.TryGetProperty("interrupted", out var interrupted)
                        && interrupted.ValueKind == JsonValueKind.True)
                    {
                        ResetPlayback("Gemini speech was interrupted by a new turn.");
                    }
                    var receivedAudio = ReadAudio(serverContent);
                    var turnComplete = serverContent.TryGetProperty("turnComplete", out var complete)
                                       && complete.ValueKind == JsonValueKind.True;
                    if (receivedAudio || turnComplete)
                    {
                        EnsurePlaybackStarted(force: turnComplete);
                    }
                }

                if (root.TryGetProperty("toolCall", out var toolCall))
                {
                    await HandleToolCallsAsync(toolCall, cancellationToken).ConfigureAwait(false);
                }

                if (root.TryGetProperty("goAway", out var goAway))
                {
                    StateChanged?.Invoke("RECONNECTING");
                    Diagnostic?.Invoke($"Gemini requested graceful reconnection: {goAway}");
                    return;
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            _setupCompletion?.TrySetException(exception);
            Diagnostic?.Invoke($"Stable Gemini Live connection error: {exception.Message}");
        }
        finally
        {
            SpeechModeCoordinator.SetLiveVoiceActive(false);
            StateChanged?.Invoke("DISCONNECTED");
        }
    }

    private void ReadTranscriptions(JsonElement serverContent)
    {
        if (serverContent.TryGetProperty("inputTranscription", out var input)
            && input.TryGetProperty("text", out var inputText)
            && !string.IsNullOrWhiteSpace(inputText.GetString()))
        {
            InputTranscript?.Invoke(inputText.GetString()!);
        }
        if (serverContent.TryGetProperty("outputTranscription", out var output)
            && output.TryGetProperty("text", out var outputText)
            && !string.IsNullOrWhiteSpace(outputText.GetString()))
        {
            OutputTranscript?.Invoke(outputText.GetString()!);
        }
    }

    private bool ReadAudio(JsonElement serverContent)
    {
        if (!serverContent.TryGetProperty("modelTurn", out var modelTurn)
            || !modelTurn.TryGetProperty("parts", out var parts)) return false;

        var received = false;
        foreach (var part in parts.EnumerateArray())
        {
            if (!part.TryGetProperty("inlineData", out var inlineData)
                || !inlineData.TryGetProperty("data", out var audioData)) continue;
            var encoded = audioData.GetString();
            if (string.IsNullOrWhiteSpace(encoded)) continue;
            var bytes = Convert.FromBase64String(encoded);
            var buffer = _playbackBuffer;
            var speaker = _speaker;
            if (buffer is null || speaker is null || bytes.Length == 0) continue;

            if (_playbackStarted && buffer.BufferedDuration < TimeSpan.FromMilliseconds(25))
            {
                try { speaker.Pause(); } catch { }
                _playbackStarted = false;
                var count = Interlocked.Increment(ref _playbackUnderruns);
                if (count <= 3 || count % 10 == 0)
                {
                    Diagnostic?.Invoke($"Voice jitter rebuffer #{count}; waiting for a continuous audio window instead of playing chopped fragments.");
                }
            }

            TrimPlaybackBacklog(buffer);
            try
            {
                buffer.AddSamples(bytes, 0, bytes.Length);
            }
            catch (InvalidOperationException)
            {
                TrimPlaybackBacklog(buffer, force: true);
                buffer.AddSamples(bytes, 0, bytes.Length);
            }

            var bufferedMilliseconds = Math.Max(0L, (long)buffer.BufferedDuration.TotalMilliseconds);
            ExtendSpeakerActive(bufferedMilliseconds + 220L);
            received = true;
        }
        return received;
    }

    private void EnsurePlaybackStarted(bool force)
    {
        if (_playbackStarted) return;
        var buffer = _playbackBuffer;
        var speaker = _speaker;
        if (buffer is null || speaker is null || buffer.BufferedBytes == 0) return;

        var threshold = force ? TimeSpan.FromMilliseconds(30) : TimeSpan.FromMilliseconds(170);
        if (buffer.BufferedDuration < threshold) return;

        try
        {
            speaker.Play();
            _playbackStarted = true;
        }
        catch (Exception exception)
        {
            Diagnostic?.Invoke($"Audio output could not start: {exception.Message}");
        }
    }

    private void TrimPlaybackBacklog(BufferedWaveProvider buffer, bool force = false)
    {
        var trigger = force ? TimeSpan.FromSeconds(1.2) : TimeSpan.FromSeconds(3.2);
        if (buffer.BufferedDuration <= trigger) return;

        var bytesPerSecond = buffer.WaveFormat.AverageBytesPerSecond;
        var targetBytes = (int)(bytesPerSecond * 0.75);
        var removable = Math.Max(0, buffer.BufferedBytes - targetBytes);
        removable -= removable % Math.Max(1, buffer.WaveFormat.BlockAlign);
        if (removable <= 0) return;

        var scratch = new byte[Math.Min(32 * 1024, removable)];
        var remaining = removable;
        while (remaining > 0)
        {
            var read = buffer.Read(scratch, 0, Math.Min(scratch.Length, remaining));
            if (read <= 0) break;
            remaining -= read;
            Interlocked.Add(ref _trimmedPlaybackBytes, read);
        }
        Diagnostic?.Invoke(
            $"Excess stale voice latency was reduced by {(removable - remaining) / 1000d:F1} KB while preserving the newest continuous speech window.");
    }

    private void ResetPlayback(string reason)
    {
        var speaker = _speaker;
        var buffer = _playbackBuffer;
        try { speaker?.Pause(); } catch { }
        buffer?.ClearBuffer();
        _playbackStarted = false;
        Interlocked.Exchange(ref _speakerActiveUntilTicks, 0);
        Diagnostic?.Invoke(reason);
    }

    private void ExtendSpeakerActive(long milliseconds)
    {
        var candidate = Environment.TickCount64 + milliseconds;
        while (true)
        {
            var current = Interlocked.Read(ref _speakerActiveUntilTicks);
            if (current >= candidate) return;
            if (Interlocked.CompareExchange(ref _speakerActiveUntilTicks, candidate, current) == current) return;
        }
    }

    private async Task HandleToolCallsAsync(JsonElement toolCall, CancellationToken cancellationToken)
    {
        if (!toolCall.TryGetProperty("functionCalls", out var calls) || ToolExecutor is null) return;
        var responses = new JsonArray();
        foreach (var call in calls.EnumerateArray())
        {
            var name = call.GetProperty("name").GetString() ?? "unknown";
            var id = call.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var args = call.TryGetProperty("args", out var argsElement)
                ? argsElement.Clone()
                : JsonDocument.Parse("{}").RootElement.Clone();
            string result;
            try
            {
                result = await ToolExecutor(name, args, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                result = $"Tool failed: {exception.Message}";
            }
            OperationalContextStore.RecordTool(name, args, result);
            responses.Add(new JsonObject
            {
                ["name"] = name,
                ["id"] = id,
                ["response"] = new JsonObject { ["result"] = result }
            });
        }
        await SendJsonNodeAsync(new JsonObject
        {
            ["toolResponse"] = new JsonObject { ["functionResponses"] = responses }
        }, cancellationToken).ConfigureAwait(false);
    }

    private Task SendJsonAsync(object message, CancellationToken cancellationToken) =>
        SendJsonNodeAsync(JsonSerializer.SerializeToNode(message)!, cancellationToken);

    private async Task SendJsonNodeAsync(JsonNode message, CancellationToken cancellationToken)
    {
        var socket = _socket ?? throw new InvalidOperationException("Gemini Live socket is not available.");
        var bytes = Encoding.UTF8.GetBytes(message.ToJsonString());
        await _sendGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _sendGate.Release();
        }
    }

    private static async Task<string?> ReceiveTextMessageAsync(
        ClientWebSocket socket,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        using var stream = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken)
                .ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close) return null;
            stream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage) return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    private void DrainMediaQueues()
    {
        while (_audioQueue.Reader.TryRead(out _)) { }
        while (_visualQueue.Reader.TryRead(out _)) { }
        Interlocked.Exchange(ref _queuedAudioPackets, 0);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _sendGate.Dispose();
    }
}
