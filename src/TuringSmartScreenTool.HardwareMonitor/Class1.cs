using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenHardwareMonitor.Hardware;

namespace TuringSmartScreenTool.HardwareMonitor
{
    //public enum Unit
    //{
    //    Volt,
    //    Watt,
    //    MHz,
    //    Celsius,
    //    Percent,
    //    RotationsPerMinute,
    //    LittersPerHour,
    //    Second,
    //    GiB,            // 2^30 Bytes
    //    MiB,            // 2^20 Bytes
    //    MiBPerSecond,   // 2^20 Bytes/sec
    //    None,
    //}

    //public enum HardwareType
    //{
    //                    // OpenHardwareMonitor.Hardware.HardwareType
    //    Motherboard,    //   Mainboard, SuperIO
    //    CPU,            //   CPU
    //    Memory,         //   RAM
    //    GPU,            //   GpuNvidia, GpuAti
    //    Network,        //   Network
    //    Storage,        //   HDD
    //    Other           //   TBalancer, Heatmaster
    //}

    // TreeViewで表示して選択させる

    public class HardwareMonitor : IDisposable
    {
        private readonly ILogger<HardwareMonitor> _logger;
        private readonly Computer _computer;

        public HardwareMonitor(ILoggerFactory loggerFactory)
        {
            OpenHardwareMonitorLib.Logging.LoggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<HardwareMonitor>();

            _computer = new Computer()
            {
                CPUEnabled = true,
                FanControllerEnabled = true,
                GPUEnabled = true,
                HDDEnabled = true,
                MainboardEnabled = true,
                NetworkEnabled = true,
                RAMEnabled = true
            };
        }

        public void Dispose()
        {
            _computer.Close();
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                _computer.Open();

                void updateHardware(IHardware[] hardwares)
                {
                    foreach (var hardware in hardwares)
                    {
                        hardware.Update();
                        var sub = hardware.SubHardware;
                        if (sub is not null)
                            updateHardware(sub);
                    }
                }

                updateHardware(_computer.Hardware);
            });
        }
    }

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

        //public HardwareInfoRepository()
        //{
        //}
    }
}
