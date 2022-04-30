using OpenHardwareMonitor.Hardware;

namespace TuringSmartScreenTool.Entities
{
    public interface IHardwareInfo
    {
        public string Id { get; }
        public string Name { get; }
        public HardwareType Type { get; }
    }
}
