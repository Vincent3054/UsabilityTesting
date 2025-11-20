using UsabilityTesting.Worker.Models;

namespace UsabilityTesting.Worker.Interfaces;

public interface ITargetProvider
{
    Task<IEnumerable<MonitorTarget>> GetTargetsAsync();
}
