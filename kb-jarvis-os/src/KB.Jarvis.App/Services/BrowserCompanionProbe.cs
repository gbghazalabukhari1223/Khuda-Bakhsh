using System.Net.Http;

namespace KB.Jarvis.App.Services;

public sealed record BrowserCompanionHealth(bool Connected, int? Port, string Detail);

public sealed class BrowserCompanionProbe
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromMilliseconds(700)
    };

    public async Task<BrowserCompanionHealth> CheckAsync(CancellationToken cancellationToken)
    {
        for (var port = 32145; port <= 32155; port++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(
                    $"http://127.0.0.1:{port}/health",
                    cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return new BrowserCompanionHealth(
                    true,
                    port,
                    string.IsNullOrWhiteSpace(raw) ? "Health endpoint responded." : raw);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                // Probe the next local bridge port.
            }
        }

        return new BrowserCompanionHealth(
            false,
            null,
            "No compatible local Browser Companion bridge responded on ports 32145–32155.");
    }
}
