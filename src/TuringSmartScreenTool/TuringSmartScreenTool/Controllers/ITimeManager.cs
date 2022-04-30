using System;

namespace TuringSmartScreenTool.Controllers
{
    public interface ITimeData
    {
        IObservable<DateTimeOffset> Value { get; }
    }

    public interface ITimeManager
    {
        ITimeData Get();
    }
}
