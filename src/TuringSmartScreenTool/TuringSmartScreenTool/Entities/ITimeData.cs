using System;

namespace TuringSmartScreenTool.Entities
{
    public interface ITimeData
    {
        IObservable<DateTimeOffset> Value { get; }
    }
}
