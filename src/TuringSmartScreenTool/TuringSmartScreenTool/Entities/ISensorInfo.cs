using System;

namespace TuringSmartScreenTool.Entities
{
    public enum ValueUnitType
    {
        None,
        Voltage,
        Hz,
        Celsius,
        Percentage,
        RotationPerMinute,
        LiterPerHour,
        Watt,
        Byte,
        BytePerSecond,
        Second
    }

    public interface ISensorInfo
    {
        public string Id { get; }
        public string Name { get; }
        public IHardwareInfo Parent { get; }
        public ValueUnitType ValueUnit { get; }
        public IObservable<double?> Value { get; }
    }
}
