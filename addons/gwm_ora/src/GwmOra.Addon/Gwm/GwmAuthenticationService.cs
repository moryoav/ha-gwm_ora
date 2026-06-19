using GwmOra.Addon.Configuration;
using libgwmapi;
using libgwmapi.DTO.UserAuth;

namespace GwmOra.Addon.Gwm;

public sealed class GwmAuthenticationService
{
    private readonly AddonOptions _options;
    private readonly AddonStateStore _stateStore;
    private readonly ILogger<GwmAuthenticationService> _logger;

    public GwmAuthenticationService(
        AddonOptions options,
        AddonStateStore stateStore,
        ILogger<GwmAuthenticationService> logger)
    {
        _options = options;
        _stateStore = stateStore;
        _logger = logger;
    }

    public async Task EnsureAuthenticatedAsync(GwmApiClient client, CancellationToken cancellationToken)
    {
        if (!String.IsNullOrWhiteSpace(_stateStore.State.AccessToken))
        {
            client.SetAccessToken(_stateStore.State.AccessToken);
            try
            {
                await client.GetUserBaseInfoAsync(cancellationToken);
                return;
            }
            catch (GwmApiException ex)
            {
                _logger.LogInformation("Stored GWM access token rejected: {Code} {Message}", ex.Code, ex.Message);
            }
        }

        if (!String.IsNullOrWhiteSpace(_stateStore.State.RefreshToken))
        {
            try
            {
                await RefreshTokenAsync(client, cancellationToken);
                return;
            }
            catch (GwmApiException ex)
            {
                _logger.LogWarning("GWM token refresh failed: {Code} {Message}", ex.Code, ex.Message);
            }
        }

        await LoginAsync(client, cancellationToken);
    }

    private async Task RefreshTokenAsync(GwmApiClient client, CancellationToken cancellationToken)
    {
        var request = new RefreshTokenRequest
        {
            DeviceId = _stateStore.State.DeviceId,
            AccessToken = _stateStore.State.AccessToken,
            RefreshToken = _stateStore.State.RefreshToken
        };

        client.SetAccessToken(String.Empty);
        var response = await client.RefreshTokenAsync(request, cancellationToken);
        await _stateStore.UpdateAsync(state =>
        {
            state.AccessToken = response.AccessToken;
            state.RefreshToken = response.RefreshToken;
        }, cancellationToken);
        client.SetAccessToken(response.AccessToken);
    }

    private async Task LoginAsync(GwmApiClient client, CancellationToken cancellationToken)
    {
        var request = new LoginAccountRequest
        {
            Country = _options.Country,
            IsEncrypt = false,
            DeviceId = _stateStore.State.DeviceId,
            Model = "ha-gwm-ora",
            PushToken = String.Empty,
            Account = _options.Username,
            Password = _options.Password
        };

        try
        {
            var response = await client.LoginAccountAsync(request, cancellationToken);
            await _stateStore.UpdateAsync(state =>
            {
                state.AccessToken = response.AccessToken;
                state.RefreshToken = response.RefreshToken;
                state.GwId = response.GwId;
                state.BeanId = response.BeanId;
            }, cancellationToken);
            client.SetAccessToken(response.AccessToken);
        }
        catch (GwmApiException ex) when (ex.Code == "110641")
        {
            throw new InvalidOperationException(
                "GWM requires SMS/e-mail verification for this account. Log in once with the official app or use an account that does not require interactive verification.",
                ex);
        }
    }
}
