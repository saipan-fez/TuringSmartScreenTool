using System.Threading.Tasks;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IHardwareMonitorController
    {
        void Dispose();
        ValueTask InitializeAsync();
        IMonitorTarget[] GetMonitorTargets(params MonitorTargetType[] types);
    }
}
