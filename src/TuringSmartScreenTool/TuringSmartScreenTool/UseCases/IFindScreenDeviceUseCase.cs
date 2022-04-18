using System.Collections.Generic;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public interface IFindScreenDeviceUseCase
    {
        IReadOnlyCollection<ScreenDevice> Find();
    }
}
