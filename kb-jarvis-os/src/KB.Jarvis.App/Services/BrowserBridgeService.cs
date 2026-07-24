using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KB.Jarvis.App.Services;

public sealed record BrowserCommandResult(bool Success, JsonElement? Data, string? Error);

public sealed class BrowserBridgeService : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<BrowserCommandResult>> _pending = new();
    private readonly SemaphoreSlim _sendGate = new(1, 1);
    private readonly object _socketGate = new();
    private WebApplication? _application;
    private WebSocket? _socket;

    public bool IsConnected
    {
        get
        {
            lock (_socketGate)
            {
                return _socket?.State == WebSocketState.Open;
            }
        }
    }

    public int Port { get; private set; } = 32145;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_application is not null)
        {
            return;
        }

        Exception? lastError = null;
        for (var port = 32145; port <= 32155; port++)
        {
            try
            {
                var builder = WebApplication.CreateSlimBuilder();
                builder.Logging.ClearProviders();
                builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
                var application = builder.Build();
                application.UseWebSockets(new WebSocketOptions
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(15)
                });

                var selectedPort = port;
                application.MapGet("/health", () => Results.Json(new
                {
                    status = "ok",
                    name = "KB Jarvis OS Browser Bridge",
                    developer = "KB (Khuda Bakhsh)",
                    version = Core.Identity.Version,
                    port = selectedPort,
                    connected = IsConnected
                }));

                application.Map("/ws", HandleWebSocketAsync);
                await application.StartAsync(cancellationToken).ConfigureAwait(false);
                _application = application;
                Port = selectedPort;
                return;
            }
            catch (Exception exception)
            {
                lastError = exception;
            }
        }

        throw new InvalidOperationException(
            "KB Jarvis could not reserve a local Browser Companion port from 32145 to 32155.",
            lastError);
    }

    public async Task<BrowserCommandResult> ExecuteAsync(
        string operation,
        object payload,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        WebSocket socket;
        lock (_socketGate)
        {
            socket = _socket is { State: WebSocketState.Open }
                ? _socket
                : throw new InvalidOperationException("The Browser Companion is not connected to KB Jarvis OS.");
        }

        var id = Guid.NewGuid().ToString("N");
        var completion = new TaskCompletionSource<BrowserCommandResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(id, completion))
        {
            throw new InvalidOperationException("Could not create a unique browser command id.");
        }

        try
        {
            var json = JsonSerializer.Serialize(new
            {
                type = "command",
                id,
                operation,
                payload
            });
            var bytes = Encoding.UTF8.GetBytes(json);

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

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(timeout);
            using var registration = timeoutSource.Token.Register(
                () => completion.TrySetCanceled(timeoutSource.Token));
            return await completion.Task.ConfigureAwait(false);
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    private async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var origin = context.Request.Headers["Origin"].ToString();
        if (!origin.StartsWith("chrome-extension://", StringComparison.OrdinalIgnoreCase)
            && !origin.StartsWith("extension://", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        using var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        WebSocket? previous;
        lock (_socketGate)
        {
            previous = _socket;
            _socket = socket;
        }

        if (previous is { State: WebSocketState.Open })
        {
            try
            {
                await previous.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "New companion connected",
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // The new connection remains authoritative.
            }
        }

        try
        {
            await ReceiveLoopAsync(socket, context.RequestAborted).ConfigureAwait(false);
        }
        finally
        {
            lock (_socketGate)
            {
                if (ReferenceEquals(_socket, socket))
                {
                    _socket = null;
                }
            }
        }
    }

    private async Task ReceiveLoopAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        var segment = new ArraySegment<byte>(buffer);

        while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            using var stream = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(segment, cancellationToken).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                stream.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Text)
            {
                continue;
            }

            using var document = JsonDocument.Parse(stream.ToArray());
            var root = document.RootElement;
            if (!root.TryGetProperty("type", out var type)
                || !string.Equals(type.GetString(), "result", StringComparison.OrdinalIgnoreCase)
                || !root.TryGetProperty("id", out var idElement))
            {
                continue;
            }

            var id = idElement.GetString();
            if (id is null || !_pending.TryGetValue(id, out var completion))
            {
                continue;
            }

            var success = root.TryGetProperty("success", out var successElement)
                          && successElement.GetBoolean();
            JsonElement? data = root.TryGetProperty("data", out var dataElement)
                ? dataElement.Clone()
                : null;
            var error = root.TryGetProperty("error", out var errorElement)
                ? errorElement.GetString()
                : null;
            completion.TrySetResult(new BrowserCommandResult(success, data, error));
        }
    }

    public async ValueTask DisposeAsync()
    {
        WebSocket? socket;
        lock (_socketGate)
        {
            socket = _socket;
            _socket = null;
        }

        if (socket is { State: WebSocketState.Open })
        {
            try
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "KB Jarvis is closing",
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Ignore shutdown transport errors.
            }
        }

        if (_application is not null)
        {
            await _application.StopAsync().ConfigureAwait(false);
            await _application.DisposeAsync().ConfigureAwait(false);
            _application = null;
        }

        _sendGate.Dispose();
    }
}