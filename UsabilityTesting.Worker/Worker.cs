using Microsoft.Extensions.Options;
using UsabilityTesting.Worker.Interfaces;
using UsabilityTesting.Worker.Models;
using UsabilityTesting.Worker.Services;

namespace UsabilityTesting.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITargetProvider _targetProvider;
    private readonly HttpMonitor _httpMonitor;
    private readonly StatusTracker _statusTracker;
    private readonly MonitorSettings _settings;

    public Worker(
        ILogger<Worker> logger,
        ITargetProvider targetProvider,
        HttpMonitor httpMonitor,
        StatusTracker statusTracker,
        IOptions<MonitorSettings> settings)
    {
        _logger = logger;
        _targetProvider = targetProvider;
        _httpMonitor = httpMonitor;
        _statusTracker = statusTracker;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Usability Monitor Worker started at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting check cycle...");

            try
            {
                var targets = await _targetProvider.GetTargetsAsync();
                
                // Run checks in parallel or sequence? 
                // Sequence is safer to avoid overwhelming the network/CPU if many targets, 
                // but parallel is faster. Let's do parallel with a limit if needed.
                // For now, simple foreach is fine for a "small program".
                
                foreach (var target in targets)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    _logger.LogInformation("Checking {Name} ({Url})...", target.Name, target.Url);
                    
                    var result = await _httpMonitor.CheckTargetAsync(target);
                    await _statusTracker.ProcessResultAsync(target, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during check cycle.");
            }

            _logger.LogInformation("Cycle completed. Waiting {Seconds} seconds...", _settings.CheckIntervalSeconds);
            await Task.Delay(_settings.CheckIntervalSeconds * 1000, stoppingToken);
        }
    }
}
