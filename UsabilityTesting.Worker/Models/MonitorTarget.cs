namespace UsabilityTesting.Worker.Models;

public class MonitorTarget
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Headers { get; set; } = string.Empty; // JSON or Key:Value;Key:Value
    public string Body { get; set; } = string.Empty;
    public int ExpectedStatusCode { get; set; } = 200;
    public string NotifyEmails { get; set; } = string.Empty; // Semicolon separated
}
