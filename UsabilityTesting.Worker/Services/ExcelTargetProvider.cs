using Microsoft.Extensions.Options;
using MiniExcelLibs;
using UsabilityTesting.Worker.Interfaces;
using UsabilityTesting.Worker.Models;

namespace UsabilityTesting.Worker.Services;

public class ExcelTargetProvider : ITargetProvider
{
    private readonly MonitorSettings _settings;
    private readonly ILogger<ExcelTargetProvider> _logger;

    public ExcelTargetProvider(IOptions<MonitorSettings> settings, ILogger<ExcelTargetProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<MonitorTarget>> GetTargetsAsync()
    {
        try
        {
            var rows = await MiniExcel.QueryAsync<MonitorTarget>(_settings.ExcelFilePath);
            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading Excel file at {Path}", _settings.ExcelFilePath);
            return Enumerable.Empty<MonitorTarget>();
        }
    }
}
