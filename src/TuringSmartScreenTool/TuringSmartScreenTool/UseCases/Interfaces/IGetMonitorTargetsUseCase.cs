using System.Threading.Tasks;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases.Interfaces
{
    public interface IGetMonitorTargetsUseCase
    {
        Task<IMonitorTarget[]> GetMonitorTargetsAsync(params MonitorTargetType[] types);
    }
}
