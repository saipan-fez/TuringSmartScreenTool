using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers
{
    public interface IHardwareFinder
    {
        IHardwareInfo Find(string id);
    }
}
