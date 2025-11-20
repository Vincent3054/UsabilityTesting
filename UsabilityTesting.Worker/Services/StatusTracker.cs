using System.Collections.Concurrent;
using UsabilityTesting.Worker.Models;

namespace UsabilityTesting.Worker.Services;

public class StatusTracker
{
    // Key: Target Name (or URL if name not unique), Value: IsHealthy
    private readonly ConcurrentDictionary<string, bool> _statusMap = new();
    private readonly EmailNotifier _emailNotifier;
    private readonly ILogger<StatusTracker> _logger;

    public StatusTracker(EmailNotifier emailNotifier, ILogger<StatusTracker> logger)
    {
        _emailNotifier = emailNotifier;
        _logger = logger;
    }

    public async Task ProcessResultAsync(MonitorTarget target, MonitorResult result)
    {
        // Use Name as key, fallback to URL
        var key = string.IsNullOrWhiteSpace(target.Name) ? target.Url : target.Name;

        // Get previous status (default to true/healthy if not present, so we alert on first failure)
        // Actually, if it's the first run and it fails, we SHOULD alert.
        // If it's the first run and it succeeds, we do nothing.
        // So default "previous" status should be considered "Healthy" effectively.
        bool previousHealthy = _statusMap.GetOrAdd(key, true);

        if (result.IsHealthy != previousHealthy)
        {
            // Status changed!
            _logger.LogInformation("Status changed for {Name}: {Old} -> {New}", key, previousHealthy ? "Healthy" : "Unhealthy", result.IsHealthy ? "Healthy" : "Unhealthy");
            
            // Update map
            _statusMap[key] = result.IsHealthy;

            // Send Alert
            string subject = result.IsHealthy 
                ? $"[Resolved] Service {key} is back online" 
                : $"[Alert] Service {key} is Unhealthy";

            string body = $@"
<h3>Monitor Alert: {key}</h3>
<p><strong>Status:</strong> {(result.IsHealthy ? "<span style='color:green'>Healthy</span>" : "<span style='color:red'>Unhealthy</span>")}</p>
<p><strong>URL:</strong> {target.Url}</p>
<p><strong>Message:</strong> {result.Message}</p>
<p><strong>Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
";
            await _emailNotifier.SendAlertAsync(subject, body, target.NotifyEmails);
        }
        else
        {
            // Status unchanged, do nothing
            _logger.LogDebug("Status unchanged for {Name}: {Status}", key, result.IsHealthy);
        }
    }
}
