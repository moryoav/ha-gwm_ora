using GwmOra.Addon.Configuration;

namespace GwmOra.Addon.Gwm;

public sealed class VehiclePollingWorker : BackgroundService
{
    private readonly AddonOptions _options;
    private readonly GwmVehicleService _vehicleService;
    private readonly ILogger<VehiclePollingWorker> _logger;

    public VehiclePollingWorker(
        AddonOptions options,
        GwmVehicleService vehicleService,
        ILogger<VehiclePollingWorker> logger)
    {
        _options = options;
        _vehicleService = vehicleService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RefreshIgnoringFailuresAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RefreshIgnoringFailuresAsync(stoppingToken);
        }
    }

    private async Task RefreshIgnoringFailuresAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _vehicleService.RefreshNowAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GWM ORA poll failed; the next poll will retry");
        }
    }
}
