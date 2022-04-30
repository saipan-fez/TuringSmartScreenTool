using System.Threading.Tasks;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers
{
    public interface IHardwareMonitorController
    {
        void Dispose();
        ValueTask InitializeAsync();
        IMonitorTarget[] GetMonitorTargets(params MonitorTargetType[] types);
    }
}
