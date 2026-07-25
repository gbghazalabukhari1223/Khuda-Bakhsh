using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using NAudio.Wave;

namespace KB.Jarvis.App.Services;

public sealed class GeminiLiveService : IAsyncDisposable
{
    private sealed record VisualPacket(byte[] Jpeg, string Source);

    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly Channel<byte[]> _audioQueue = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(30)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.DropOldest
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

    public event Action<string>? StateChanged;
    public event Action<string>? InputTranscript;
    public event Action<string>? OutputTranscript;
    public event Action<string>? Diagnostic;

    public Func<string, JsonElement, CancellationToken, Task<string>>? ToolExecutor { get; set; }
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

        _mediaSendTask = Task.Run(() => MediaSendLoopAsync(token), token);
        StartAudioDevices();
        StateChanged?.Invoke("LISTENING");
        Diagnostic?.Invoke("Fluent Live voice ready: short bounded microphone packets, echo suppression, session compression and resumption are active.");
    }

    public async Task StopAsync()
    {
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
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Jarvis voice stopped", CancellationToken.None)
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
                    parts = new[] { new { text = GeminiToolCatalog.SystemInstruction } }
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
            BufferDuration = TimeSpan.FromSeconds(3),
            DiscardOnBufferOverflow = true,
            ReadFully = true
        };
        _speaker = new WaveOutEvent
        {
            DesiredLatency = 70,
            NumberOfBuffers = 4
        };
        _speaker.Init(_playbackBuffer);
        _speaker.Play();

        _microphone = new WaveInEvent
        {
            WaveFormat = new WaveFormat(16000, 16, 1),
            BufferMilliseconds = 40,
            NumberOfBuffers = 4
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
        _audioQueue.Writer.TryWrite(copy);
    }

    private async Task MediaSendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var workDone = false;
                var audioBurst = 0;
                while (audioBurst < 8 && _audioQueue.Reader.TryRead(out var audio))
                {
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
            Diagnostic?.Invoke($"Media send loop error: {exception.Message}");
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
                        Diagnostic?.Invoke("Gemini Live session resumption handle refreshed.");
                    }
                }

                if (root.TryGetProperty("serverContent", out var serverContent))
                {
                    ReadTranscriptions(serverContent);
                    ReadAudio(serverContent);
                }
                if (root.TryGetProperty("toolCall", out var toolCall))
                {
                    await HandleToolCallsAsync(toolCall, cancellationToken).ConfigureAwait(false);
                }
                if (root.TryGetProperty("goAway", out var goAway))
                {
                    StateChanged?.Invoke("RECONNECTING");
                    Diagnostic?.Invoke($"Gemini requested graceful reconnection: {goAway}");
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            _setupCompletion?.TrySetException(exception);
            Diagnostic?.Invoke($"Gemini Live connection error: {exception.Message}");
        }
        finally
        {
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
            || !modelTurn.TryGetProperty("parts", out var parts)) return;
        foreach (var part in parts.EnumerateArray())
        {
            if (!part.TryGetProperty("inlineData", out var inlineData)
                || !inlineData.TryGetProperty("data", out var audioData)) continue;
            var bytes = Convert.FromBase64String(audioData.GetString() ?? string.Empty);
            var buffer = _playbackBuffer;
            if (buffer is null) continue;
            if (buffer.BufferedDuration > TimeSpan.FromSeconds(2.2))
            {
                buffer.ClearBuffer();
                Diagnostic?.Invoke("Stale speech backlog cleared to keep Jarvis synchronized.");
            }
            buffer.AddSamples(bytes, 0, bytes.Length);
            var durationMilliseconds = Math.Max(20L, bytes.Length * 1000L / (24000L * 2L));
            ExtendSpeakerActive(durationMilliseconds + 140L);
        }
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
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _sendGate.Release();
        }
    }

    private static async Task<string?> ReceiveTextMessageAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        using var stream = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close) return null;
            stream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage) return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    private void DrainMediaQueues()
    {
        while (_audioQueue.Reader.TryRead(out _)) { }
        while (_visualQueue.Reader.TryRead(out _)) { }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _sendGate.Dispose();
    }
}
