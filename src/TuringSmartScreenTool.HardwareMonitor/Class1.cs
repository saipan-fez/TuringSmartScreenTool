using System.Diagnostics;

namespace TuringSmartScreenTool.HardwareMonitor
{
    // HW意外に取得するもの
    //   天気
    //   日時

    public enum MonitorType
    {
        // Motherboard
        MotherboardName,
        MotherboardTemperature,

        // CPU
        CpuName,
        CpuUsageRate,
        CpuClock,
        CpuPackagePower,
        CpuTemperature,
        CpuFanName,

        // FAN
        GenericFanName,
        GenericFanSpeed,

        // Memory
        Memory, // (free/usage/total)
        MemoryFrequency,

        // GPU
        GpuName,
        GpuClock,
        GpuUsageRate,
        GpuMemoryControllerUsage,
        GpuVram, // (free/usage/total)
        GpuTemperature,
        GpuFanSpeed,
        GpuPower,

        // Disk
        StorageName,
        StorageUsageRate,
        StorageTotalSize,
        StorageUsageSize,
        StorageTemperature,

        // Network
        NetworkAdapterName,
        NetworkAdapterIPv4Address,
        NetworkAdapterIPv6Address,
        NetworkAdapterReceive,
        NetworkAdapterSend,
    }

    // sigleton
    public class HardwareMintorFactory
    {
        private PerformanceCounter _cpuCounter = new PerformanceCounter();

        public HardwareInfoRepository()
        {
        }
    }
}
