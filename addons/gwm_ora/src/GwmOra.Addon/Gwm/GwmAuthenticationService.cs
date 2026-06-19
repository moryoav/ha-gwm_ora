using GwmOra.Addon.Configuration;
using GwmOra.Addon.Supervisor;
using libgwmapi;
using libgwmapi.DTO.UserAuth;

namespace GwmOra.Addon.Gwm;

public sealed class GwmAuthenticationService
{
    private static readonly TimeSpan VerificationCodeRequestInterval = TimeSpan.FromMinutes(10);

    private readonly AddonOptions _options;
    private readonly AddonStateStore _stateStore;
    private readonly SupervisorOptionsService _supervisorOptions;
    private readonly ILogger<GwmAuthenticationService> _logger;

    public GwmAuthenticationService(
        AddonOptions options,
        AddonStateStore stateStore,
        SupervisorOptionsService supervisorOptions,
        ILogger<GwmAuthenticationService> logger)
    {
        _options = options;
        _stateStore = stateStore;
        _supervisorOptions = supervisorOptions;
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
            await StoreLoginResponseAsync(client, response, cancellationToken);
        }
        catch (GwmApiException ex) when (IsVerificationRequired(ex))
        {
            if (!String.IsNullOrWhiteSpace(_options.VerificationCode))
            {
                await LoginWithVerificationCodeAsync(client, cancellationToken);
                return;
            }

            await RequestVerificationCodeAsync(client, cancellationToken);
            throw new GwmVerificationRequiredException(
                "GWM requires SMS/e-mail verification. A verification code was requested; enter it in the add-on option 'verification_code', save, and restart the add-on.",
                ex);
        }
    }

    private async Task LoginWithVerificationCodeAsync(GwmApiClient client, CancellationToken cancellationToken)
    {
        var request = new LoginWithSmsRequest
        {
            Email = _options.Username,
            Country = _options.Country,
            DeviceId = _stateStore.State.DeviceId,
            Model = "ha-gwm-ora",
            PushToken = String.Empty,
            SmsCode = _options.VerificationCode!
        };

        try
        {
            var response = await client.LoginWithSmsAsync(request, cancellationToken);
            await StoreLoginResponseAsync(client, response, cancellationToken);
            await _supervisorOptions.ClearVerificationCodeAsync(cancellationToken);
            _logger.LogInformation("GWM verification code accepted and tokens stored");
        }
        catch (GwmApiException ex)
        {
            await _stateStore.UpdateAsync(state =>
            {
                state.VerificationCodeRequestedAt = null;
            }, cancellationToken);
            throw new GwmVerificationRequiredException(
                "GWM rejected the configured verification_code. Clear it, restart the add-on to request a fresh code, then enter the new code and restart again.",
                ex);
        }
    }

    private async Task RequestVerificationCodeAsync(GwmApiClient client, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var lastRequest = _stateStore.State.VerificationCodeRequestedAt;
        if (lastRequest.HasValue && now - lastRequest.Value < VerificationCodeRequestInterval)
        {
            return;
        }

        try
        {
            await client.GetSmsCodeAsync(new GetSmsCode { Email = _options.Username }, cancellationToken);
            await _stateStore.UpdateAsync(state =>
            {
                state.VerificationCodeRequestedAt = now;
            }, cancellationToken);
            _logger.LogWarning("GWM requested account verification; a verification code was sent to the account e-mail/SMS channel");
        }
        catch (GwmApiException ex)
        {
            throw new GwmVerificationRequiredException(
                $"GWM requires SMS/e-mail verification, but requesting a verification code failed: {ex.Message}",
                ex);
        }
    }

    private async Task StoreLoginResponseAsync(
        GwmApiClient client,
        LoginAccountResponse response,
        CancellationToken cancellationToken)
    {
        await _stateStore.UpdateAsync(state =>
        {
            state.AccessToken = response.AccessToken;
            state.RefreshToken = response.RefreshToken;
            state.GwId = response.GwId;
            state.BeanId = response.BeanId;
            state.VerificationCodeRequestedAt = null;
        }, cancellationToken);
        client.SetAccessToken(response.AccessToken);
    }

    private static bool IsVerificationRequired(GwmApiException ex)
    {
        return ex.Code == "110641" ||
               ex.Message.Contains("verification code", StringComparison.OrdinalIgnoreCase);
    }
}
