using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GwmOra.Addon.Supervisor;

public sealed class SupervisorOptionsService
{
    private readonly ILogger<SupervisorOptionsService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public SupervisorOptionsService(ILogger<SupervisorOptionsService> logger)
    {
        _logger = logger;
    }

    public async Task ClearVerificationCodeAsync(CancellationToken cancellationToken)
    {
        var supervisorToken = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
        if (String.IsNullOrWhiteSpace(supervisorToken))
        {
            return;
        }

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://supervisor/") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supervisorToken);

            using var infoResponse = await client.GetAsync("addons/self/info", cancellationToken);
            infoResponse.EnsureSuccessStatusCode();

            await using var stream = await infoResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!document.RootElement.TryGetProperty("data", out var data) ||
                !data.TryGetProperty("options", out var optionsElement))
            {
                return;
            }

            var options = JsonNode.Parse(optionsElement.GetRawText())?.AsObject();
            if (options is null || !options.Remove("verification_code"))
            {
                return;
            }

            var payload = new JsonObject { ["options"] = options };
            using var content = new StringContent(payload.ToJsonString(_jsonOptions), Encoding.UTF8, "application/json");
            using var updateResponse = await client.PostAsync("addons/self/options", content, cancellationToken);
            updateResponse.EnsureSuccessStatusCode();
            _logger.LogInformation("Cleared one-time GWM verification code from add-on options");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not clear one-time GWM verification code from add-on options");
        }
    }
}
