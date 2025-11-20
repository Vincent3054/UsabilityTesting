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
            return rows.ToList();
        }
        catch (System.IO.InvalidDataException)
        {
            // Fallback: Try reading as CSV if it's not a valid Zip (XLSX)
            _logger.LogWarning("File {Path} is not a valid Excel (XLSX) file. Attempting to read as CSV...", _settings.ExcelFilePath);
            try 
            {
                var rows = await MiniExcel.QueryAsync<MonitorTarget>(_settings.ExcelFilePath, excelType: ExcelType.CSV);
                return rows.ToList();
            }
            catch (Exception exCsv)
            {
                _logger.LogError(exCsv, "Failed to read file as CSV as well.");
                return Enumerable.Empty<MonitorTarget>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading Excel file at {Path}", _settings.ExcelFilePath);
            return Enumerable.Empty<MonitorTarget>();
        }
    }
}
