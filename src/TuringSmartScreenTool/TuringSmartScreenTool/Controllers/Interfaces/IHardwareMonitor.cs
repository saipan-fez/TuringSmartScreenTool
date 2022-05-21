using System.Threading.Tasks;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IHardwareMonitorController
    {
        void Dispose();
        void StartToInitialize();
        Task WaitToInitializeAsync();
        Task<IMonitorTarget[]> GetMonitorTargetsAsync(params MonitorTargetType[] types);
    }
}
