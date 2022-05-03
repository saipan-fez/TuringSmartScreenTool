using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface IHardwareFinder
    {
        IHardwareInfo Find(string id);
    }
}
