using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers
{
    public interface ISensorFinder
    {
        ISensorInfo Find(string id);
    }
}
