using UsabilityTesting.Worker;
using UsabilityTesting.Worker.Interfaces;
using UsabilityTesting.Worker.Models;
using UsabilityTesting.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.Configure<MonitorSettings>(builder.Configuration.GetSection("MonitorSettings"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITargetProvider, ExcelTargetProvider>();
builder.Services.AddSingleton<HttpMonitor>();
builder.Services.AddSingleton<StatusTracker>();
builder.Services.AddSingleton<EmailNotifier>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
