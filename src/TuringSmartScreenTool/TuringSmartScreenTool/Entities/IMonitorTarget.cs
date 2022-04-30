using System.Collections.Generic;

namespace TuringSmartScreenTool.Entities
{
    public interface IMonitorTarget
    {
        public string Id { get; }
        public string Name { get; }
        public string Value { get; }
        public MonitorTargetType Type { get; }
        public IReadOnlyCollection<IMonitorTarget> Children { get; }
    }
}
