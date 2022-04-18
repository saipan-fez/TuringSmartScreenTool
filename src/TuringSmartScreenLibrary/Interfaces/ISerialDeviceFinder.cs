using System.Collections.Generic;
using TuringSmartScreenLibrary.Entities;

namespace TuringSmartScreenLibrary.Interfaces
{
    public interface ISerialDeviceFinder
    {
        IReadOnlyCollection<SerialDevice> Find();
    }
}
