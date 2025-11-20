using System.Net.Http;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using UsabilityTesting.Worker.Models;

namespace UsabilityTesting.Worker.Services;

public class HttpMonitor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MonitorSettings _settings;
    private readonly ILogger<HttpMonitor> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public HttpMonitor(IHttpClientFactory httpClientFactory, IOptions<MonitorSettings> settings, ILogger<HttpMonitor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings.Value;
        _logger = logger;

        // Define retry policy: Retry N times with delay, if result is not success or exception occurs
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Basic check, refined logic below
            .WaitAndRetryAsync(
                _settings.RetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(_settings.RetryDelayMilliseconds),
                (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for {Url} due to {Reason}", 
                        retryCount, 
                        context["Url"], 
                        outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                });
    }

    public async Task<MonitorResult> CheckTargetAsync(MonitorTarget target)
    {
        var client = _httpClientFactory.CreateClient();

        // Define a specific policy for this target's expected status code
        var policy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode != target.ExpectedStatusCode)
            .WaitAndRetryAsync(
                _settings.RetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(_settings.RetryDelayMilliseconds),
                (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount}/{MaxRetries} for {Name} ({Url}). Reason: {Reason}", 
                        retryCount, 
                        _settings.RetryCount,
                        target.Name,
                        target.Url, 
                        outcome.Exception?.Message ?? $"Status {(int)outcome.Result.StatusCode}");
                });

        try
        {
            var context = new Context();
            
            var response = await policy.ExecuteAsync(async (ctx) => 
            {
                var requestMessage = new HttpRequestMessage(new HttpMethod(target.Method), target.Url);

                // Add headers
                if (!string.IsNullOrWhiteSpace(target.Headers))
                {
                    var headers = target.Headers.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var header in headers)
                    {
                        var parts = header.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            requestMessage.Headers.TryAddWithoutValidation(parts[0].Trim(), parts[1].Trim());
                        }
                    }
                }

                // Add body
                if (!string.IsNullOrWhiteSpace(target.Body) && 
                   (target.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || target.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
                {
                    requestMessage.Content = new StringContent(target.Body, System.Text.Encoding.UTF8, "application/json");
                }

                return await client.SendAsync(requestMessage);
            }, context);

            bool isHealthy = (int)response.StatusCode == target.ExpectedStatusCode;
            
            return new MonitorResult
            {
                TargetName = target.Name,
                Url = target.Url,
                IsHealthy = isHealthy,
                StatusCode = (int)response.StatusCode,
                Message = isHealthy ? "OK" : $"Unexpected Status Code: {response.StatusCode} (Expected {target.ExpectedStatusCode})"
            };
        }
        catch (Exception ex)
        {
            return new MonitorResult
            {
                TargetName = target.Name,
                Url = target.Url,
                IsHealthy = false,
                StatusCode = 0,
                Message = $"Exception: {ex.Message}"
            };
        }
    }
}

public class MonitorResult
{
    public string TargetName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
