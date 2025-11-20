namespace UsabilityTesting.Worker.Models;

public class MonitorSettings
{
    public string ExcelFilePath { get; set; } = "config/targets.xlsx";
    public int CheckIntervalSeconds { get; set; } = 300;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 2000;
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "Usability Monitor";
}
