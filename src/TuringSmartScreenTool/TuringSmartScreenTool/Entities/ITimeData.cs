using System;

namespace TuringSmartScreenTool.Controllers.Interfaces
{
    public interface ITimeData
    {
        IObservable<DateTimeOffset> Value { get; }
    }
}
