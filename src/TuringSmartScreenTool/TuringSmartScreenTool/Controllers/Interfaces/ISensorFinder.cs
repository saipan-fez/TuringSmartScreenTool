using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface ISensorFinder
    {
        ISensorInfo Find(string id);
    }
}
