using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases.Interfaces
{
    public interface IFindHardwareInfoUseCase
    {
        IHardwareInfo Find(string id);
    }
}
