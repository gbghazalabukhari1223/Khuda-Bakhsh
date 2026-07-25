using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace KB.Jarvis.App.Services;

public sealed record VoiceHealthSnapshot(
    string OutputMode,
    int BufferedMilliseconds,
    int TargetPrebufferMilliseconds,
    long PlaybackUnderruns,
    long MicrophonePacketsDropped,
    long MicrophonePacketsSuppressed,
    long BargeIns,
    int QueuedMicrophonePackets,
    long TrimmedPlaybackBytes);

internal sealed class AdaptiveJitterWaveProvider : IWaveProvider
{
    private readonly object _gate = new();
    private readonly BufferedWaveProvider _buffer;
    private readonly int _minimumPrebufferMs;
    private readonly int _maximumPrebufferMs;
    private int _targetPrebufferMs;
    private bool _primed;
    private bool _turnCompleting;
    private long _underruns;
    private long _trimmedBytes;
    private long _lastUnderrunTicks;
    private long _lastAudioTicks;
    private long _speechStartedTicks;

    public AdaptiveJitterWaveProvider(WaveFormat format, int initialPrebufferMs = 280, int minimumPrebufferMs = 180, int maximumPrebufferMs = 650)
    {
        WaveFormat = format;
        _minimumPrebufferMs = minimumPrebufferMs;
        _maximumPrebufferMs = maximumPrebufferMs;
        _targetPrebufferMs = Math.Clamp(initialPrebufferMs, minimumPrebufferMs, maximumPrebufferMs);
        _buffer = new BufferedWaveProvider(format)
        {
            BufferDuration = TimeSpan.FromSeconds(8),
            DiscardOnBufferOverflow = false,
            ReadFully = false
        };
    }

    public WaveFormat WaveFormat { get; }

    public bool IsSpeaking
    {
        get
        {
            lock (_gate)
            {
                return _primed && _buffer.BufferedBytes > 0;
            }
        }
    }

    public long SpeechStartedTicks
    {
        get
        {
            lock (_gate)
            {
                return _speechStartedTicks;
            }
        }
    }

    public void AddSamples(byte[] data, int offset, int count)
    {
        if (count <= 0) return;
        lock (_gate)
        {
            TrimBacklogUnsafe();
            try
            {
                _buffer.AddSamples(data, offset, count);
            }
            catch (InvalidOperationException)
            {
                TrimBacklogUnsafe(force: true);
                _buffer.AddSamples(data, offset, count);
            }

            _lastAudioTicks = Environment.TickCount64;
            _turnCompleting = false;

            if (_lastUnderrunTicks > 0
                && Environment.TickCount64 - _lastUnderrunTicks > 12_000
                && _targetPrebufferMs > _minimumPrebufferMs)
            {
                _targetPrebufferMs = Math.Max(_minimumPrebufferMs, _targetPrebufferMs - 20);
                _lastUnderrunTicks = Environment.TickCount64;
            }
        }
    }

    public void MarkTurnComplete()
    {
        lock (_gate)
        {
            _turnCompleting = true;
        }
    }

    public void Interrupt()
    {
        lock (_gate)
        {
            _buffer.ClearBuffer();
            _primed = false;
            _turnCompleting = false;
            _speechStartedTicks = 0;
        }
    }

    public VoiceHealthSnapshot Snapshot(
        long dropped,
        long suppressed,
        long bargeIns,
        int queuedPackets,
        string outputMode)
    {
        lock (_gate)
        {
            return new VoiceHealthSnapshot(
                outputMode,
                (int)Math.Round(_buffer.BufferedDuration.TotalMilliseconds),
                _targetPrebufferMs,
                _underruns,
                dropped,
                suppressed,
                bargeIns,
                queuedPackets,
                _trimmedBytes);
        }
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        lock (_gate)
        {
            if (!_primed)
            {
                if (_buffer.BufferedDuration.TotalMilliseconds < _targetPrebufferMs)
                {
                    Array.Clear(buffer, offset, count);
                    return count;
                }

                _primed = true;
                _speechStartedTicks = Environment.TickCount64;
            }

            var available = _buffer.BufferedBytes;
            if (available < count)
            {
                if (_turnCompleting && available > 0)
                {
                    var readTail = _buffer.Read(buffer, offset, available);
                    if (readTail < count)
                    {
                        Array.Clear(buffer, offset + readTail, count - readTail);
                    }
                    _primed = false;
                    _turnCompleting = false;
                    _speechStartedTicks = 0;
                    return count;
                }

                _primed = false;
                _speechStartedTicks = 0;
                _underruns++;
                _lastUnderrunTicks = Environment.TickCount64;
                _targetPrebufferMs = Math.Min(_maximumPrebufferMs, _targetPrebufferMs + 55);
                Array.Clear(buffer, offset, count);
                return count;
            }

            var read = _buffer.Read(buffer, offset, count);
            if (read < count)
            {
                Array.Clear(buffer, offset + read, count - read);
            }
            return count;
        }
    }

    private void TrimBacklogUnsafe(bool force = false)
    {
        var triggerMs = force ? 1600 : 2600;
        if (_buffer.BufferedDuration.TotalMilliseconds <= triggerMs) return;

        var targetBytes = (int)(WaveFormat.AverageBytesPerSecond * 0.8);
        var removable = Math.Max(0, _buffer.BufferedBytes - targetBytes);
        removable -= removable % Math.Max(1, WaveFormat.BlockAlign);
        if (removable <= 0) return;

        var scratch = new byte[Math.Min(32 * 1024, removable)];
        var remaining = removable;
        while (remaining > 0)
        {
            var read = _buffer.Read(scratch, 0, Math.Min(scratch.Length, remaining));
            if (read <= 0) break;
            remaining -= read;
            _trimmedBytes += read;
        }
    }
}

public sealed class StableGeminiLiveService : IAsyncDisposable
{
    private sealed record VisualPacket(byte[] Jpeg, string Source);

    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly Channel<byte[]> _audioQueue = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(90)
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
    private IWavePlayer? _speaker;
    private AdaptiveJitterWaveProvider? _playback;
    private Task? _receiveTask;
    private Task? _mediaSendTask;
    private Task? _healthTask;
    private TaskCompletionSource<bool>? _setupCompletion;
    private string? _sessionHandle;
    private long _suppressedMicPackets;
    private long _droppedMicPackets;
    private long _bargeIns;
    private int _queuedAudioPackets;
    private int _bargeInCandidateFrames;
    private string _outputMode = "WASAPI shared";

    public event Action<string>? StateChanged;
    public event Action<string>? InputTranscript;
    public event Action<string>? OutputTranscript;
    public event Action<string>? Diagnostic;
    public event Action<VoiceHealthSnapshot>? HealthUpdated;

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
        Interlocked.Exchange(ref _suppressedMicPackets, 0);
        Interlocked.Exchange(ref _droppedMicPackets, 0);
        Interlocked.Exchange(ref _bargeIns, 0);
        Interlocked.Exchange(ref _queuedAudioPackets, 0);
        Interlocked.Exchange(ref _bargeInCandidateFrames, 0);

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
        setupTimeout.CancelAfter(TimeSpan.FromSeconds(20));
        await _setupCompletion.Task.WaitAsync(setupTimeout.Token).ConfigureAwait(false);

        StartAudioDevices();
        _mediaSendTask = Task.Run(() => MediaSendLoopAsync(token), token);
        _healthTask = Task.Run(() => HealthLoopAsync(token), token);
        SpeechModeCoordinator.SetLiveVoiceActive(true);
        StateChanged?.Invoke("LISTENING");
        Diagnostic?.Invoke(
            "Advanced Voice v17 ready: always-running WASAPI output, adaptive jitter buffering, 80 ms microphone batching, barge-in and voice-priority bandwidth are active.");
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
            _playback = null;
        }

        if (_mediaSendTask is not null)
        {
            try { await _mediaSendTask.ConfigureAwait(false); } catch { }
            _mediaSendTask = null;
        }

        if (_healthTask is not null)
        {
            try { await _healthTask.ConfigureAwait(false); } catch { }
            _healthTask = null;
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
                        "Jarvis Advanced Voice stopped",
                        CancellationToken.None).ConfigureAwait(false);
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
        if (jpeg.Length == 0
            || _socket?.State != WebSocketState.Open
            || cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

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
        _playback = new AdaptiveJitterWaveProvider(
            new WaveFormat(24000, 16, 1),
            initialPrebufferMs: 300,
            minimumPrebufferMs: 200,
            maximumPrebufferMs: 700);

        try
        {
            _speaker = new WasapiOut(AudioClientShareMode.Shared, true, 90);
            _outputMode = "WASAPI shared";
            _speaker.Init(_playback);
            _speaker.Play();
        }
        catch (Exception wasapiError)
        {
            try { _speaker?.Dispose(); } catch { }
            _speaker = new WaveOutEvent
            {
                DesiredLatency = 120,
                NumberOfBuffers = 4
            };
            _outputMode = "WaveOut fallback";
            _speaker.Init(_playback);
            _speaker.Play();
            Diagnostic?.Invoke($"WASAPI was unavailable; continuous WaveOut fallback activated: {wasapiError.Message}");
        }

        _microphone = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 20,
            NumberOfBuffers = 8
        };
        _microphone.DataAvailable += MicrophoneOnDataAvailable;
        _microphone.RecordingStopped += (_, args) =>
        {
            if (args.Exception is not null)
            {
                Diagnostic?.Invoke($"Microphone stream stopped: {args.Exception.Message}");
            }
        };
        _microphone.StartRecording();
    }

    private void MicrophoneOnDataAvailable(object? sender, WaveInEventArgs eventArgs)
    {
        if (_socket?.State != WebSocketState.Open
            || _sessionCts?.IsCancellationRequested != false
            || eventArgs.BytesRecorded <= 0)
        {
            return;
        }

        var playback = _playback;
        if (playback?.IsSpeaking == true)
        {
            var rms = CalculateRms(eventArgs.Buffer, eventArgs.BytesRecorded);
            var speechAge = Environment.TickCount64 - playback.SpeechStartedTicks;
            if (rms >= 0.20 && speechAge > 450)
            {
                var candidate = Interlocked.Increment(ref _bargeInCandidateFrames);
                if (candidate >= 3)
                {
                    playback.Interrupt();
                    Interlocked.Exchange(ref _bargeInCandidateFrames, 0);
                    Interlocked.Increment(ref _bargeIns);
                    Diagnostic?.Invoke("Boss barge-in detected; Jarvis speech was stopped cleanly and listening resumed.");
                }
                else
                {
                    Interlocked.Increment(ref _suppressedMicPackets);
                    return;
                }
            }
            else
            {
                Interlocked.Exchange(ref _bargeInCandidateFrames, 0);
                Interlocked.Increment(ref _suppressedMicPackets);
                return;
            }
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
        else
        {
            Interlocked.Increment(ref _droppedMicPackets);
        }
    }

    private async Task MediaSendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var first = await _audioQueue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                Interlocked.Decrement(ref _queuedAudioPackets);

                var frames = new List<byte[]>(4) { first };
                while (frames.Count < 4 && _audioQueue.Reader.TryRead(out var additional))
                {
                    Interlocked.Decrement(ref _queuedAudioPackets);
                    frames.Add(additional);
                }

                var totalLength = frames.Sum(frame => frame.Length);
                var combined = new byte[totalLength];
                var offset = 0;
                foreach (var frame in frames)
                {
                    Buffer.BlockCopy(frame, 0, combined, offset, frame.Length);
                    offset += frame.Length;
                }

                await SendAudioPacketAsync(combined, cancellationToken).ConfigureAwait(false);

                if (Volatile.Read(ref _queuedAudioPackets) == 0
                    && _visualQueue.Reader.TryRead(out var visual))
                {
                    await SendVisualPacketAsync(visual, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (ChannelClosedException) { }
        catch (Exception exception)
        {
            Diagnostic?.Invoke($"Advanced media send loop error: {exception.Message}");
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
                    }
                }

                if (root.TryGetProperty("serverContent", out var serverContent))
                {
                    ReadTranscriptions(serverContent);

                    if (serverContent.TryGetProperty("interrupted", out var interrupted)
                        && interrupted.ValueKind == JsonValueKind.True)
                    {
                        _playback?.Interrupt();
                        Diagnostic?.Invoke("Gemini interrupted its previous turn; stale speech was removed.");
                    }

                    ReadAudio(serverContent);

                    if (serverContent.TryGetProperty("turnComplete", out var complete)
                        && complete.ValueKind == JsonValueKind.True)
                    {
                        _playback?.MarkTurnComplete();
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
            Diagnostic?.Invoke($"Advanced Gemini Live connection error: {exception.Message}");
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

    private void ReadAudio(JsonElement serverContent)
    {
        if (!serverContent.TryGetProperty("modelTurn", out var modelTurn)
            || !modelTurn.TryGetProperty("parts", out var parts))
        {
            return;
        }

        foreach (var part in parts.EnumerateArray())
        {
            if (!part.TryGetProperty("inlineData", out var inlineData)
                || !inlineData.TryGetProperty("data", out var audioData))
            {
                continue;
            }

            var encoded = audioData.GetString();
            if (string.IsNullOrWhiteSpace(encoded)) continue;

            try
            {
                var bytes = Convert.FromBase64String(encoded);
                _playback?.AddSamples(bytes, 0, bytes.Length);
            }
            catch (FormatException exception)
            {
                Diagnostic?.Invoke($"Invalid audio frame was ignored: {exception.Message}");
            }
        }
    }

    private async Task HealthLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                var snapshot = _playback?.Snapshot(
                    Interlocked.Read(ref _droppedMicPackets),
                    Interlocked.Read(ref _suppressedMicPackets),
                    Interlocked.Read(ref _bargeIns),
                    Volatile.Read(ref _queuedAudioPackets),
                    _outputMode);
                if (snapshot is not null)
                {
                    HealthUpdated?.Invoke(snapshot);
                }
            }
        }
        catch (OperationCanceledException) { }
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
                cancellationToken).ConfigureAwait(false);
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
            var result = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken).ConfigureAwait(false);

            if (result.MessageType == WebSocketMessageType.Close) return null;

            stream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

    private static double CalculateRms(byte[] buffer, int count)
    {
        if (count < 2) return 0;
        double sum = 0;
        var samples = count / 2;
        for (var index = 0; index + 1 < count; index += 2)
        {
            var sample = (short)(buffer[index] | buffer[index + 1] << 8);
            var normalized = sample / 32768d;
            sum += normalized * normalized;
        }
        return Math.Sqrt(sum / Math.Max(1, samples));
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
