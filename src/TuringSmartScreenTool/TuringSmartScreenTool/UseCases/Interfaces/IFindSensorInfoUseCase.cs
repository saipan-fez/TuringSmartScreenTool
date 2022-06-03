using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases.Interfaces
{
    public interface IFindSensorInfoUseCase
    {
        ISensorInfo Find(string id);
    }
}
