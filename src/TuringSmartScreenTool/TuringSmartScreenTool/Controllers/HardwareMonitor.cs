using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenHardwareMonitor.Hardware;
using Reactive.Bindings;
using TuringSmartScreenTool.Entities;
using Humanizer;
using TuringSmartScreenTool.Helpers;
using TuringSmartScreenTool.Controllers.Interfaces;
using System.Threading;

namespace TuringSmartScreenTool.Controllers
{
    public class HardwareMonitorParameter
    {
        public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(1);
    }

    public class HardwareMonitor : IDisposable, IHardwareMonitorController, ISensorFinder, IHardwareFinder
    {
        private class SensorInfo : ISensorInfo
        {
            private readonly ReactiveProperty<double?> _value;

            public string Id { get; }
            public string Name { get; }
            public IHardwareInfo Parent { get; }
            public ValueUnitType ValueUnit { get; }
            public IObservable<double?> Value => _value;

            public SensorInfo(ISensor sensor)
            {
                _value = new(sensor.Value);

                Id = sensor.Identifier.ToString();
                Name = sensor.Name;
                ValueUnit = ToValueUnitType(sensor.SensorType);
                Parent = new HardwareInfo(sensor.Hardware);
            }

            public void UpdateValue(ISensor sensor)
            {
                _value.Value = sensor.SensorType switch
                {
                    SensorType.Clock => sensor.Value * 1000 * 1000,         // MHz   -> Hz
                    SensorType.Data => sensor.Value * 1024 * 1024 * 1024,   // GiB   -> Byte
                    SensorType.SmallData => sensor.Value * 1024 * 1024,     // MiB   -> Byte
                    SensorType.Throughput => sensor.Value * 1024 * 1024,    // MiB/s -> Byte/s
                    _ => sensor.Value,
                };
            }

            private static ValueUnitType ToValueUnitType(SensorType sensorType)
            {
                return sensorType switch
                {
                    SensorType.Voltage => ValueUnitType.Voltage,
                    SensorType.Clock => ValueUnitType.Hz,
                    SensorType.Temperature => ValueUnitType.Celsius,
                    SensorType.Load => ValueUnitType.Percentage,
                    SensorType.Fan => ValueUnitType.RotationPerMinute,
                    SensorType.Flow => ValueUnitType.LiterPerHour,
                    SensorType.Control => ValueUnitType.Percentage,
                    SensorType.Level => ValueUnitType.Percentage,
                    SensorType.Factor => ValueUnitType.None,
                    SensorType.Power => ValueUnitType.Watt,
                    SensorType.Data => ValueUnitType.Byte,
                    SensorType.SmallData => ValueUnitType.Byte,
                    SensorType.Throughput => ValueUnitType.BytePerSecond,
                    SensorType.RawValue => ValueUnitType.None,
                    SensorType.TimeSpan => ValueUnitType.Second,
                    _ => throw new InvalidOperationException(),
                };
            }
        }

        private class HardwareInfo : IHardwareInfo
        {
            public string Id { get; }
            public string Name { get; }
            public HardwareType Type { get; }

            public HardwareInfo(IHardware hardware)
            {
                Id = hardware.Identifier.ToString();
                Name = OptimizeName(hardware.Name);
                Type = hardware.HardwareType;
            }

            private string OptimizeName(string name)
            {
                // The name of the NVidia GPU provided by OpenHardwareMonitor is like "NVIDIA NVIDIA GeForce 2080Ti".
                // Replace the name to fix this bug.
                return name.Replace("NVIDIA NVIDIA", "NVIDIA");
            }
        }

        private readonly ILogger<HardwareMonitor> _logger;
        private readonly IValueUpdateManager _valueUpdateManager;
        private readonly HardwareMonitorParameter _hardwareMonitorParameter;

        private readonly Stopwatch _stopwatch;
        private readonly Computer _computer;
        private readonly Dictionary<Identifier, SensorInfo> _sensorDictionary = new();

        private Task _task = null;
        private bool _isInitialized = false;
        private bool _isDisposed = false;
        private string _valueUpdateManagerId = null;

        public HardwareMonitor(
            ILoggerFactory loggerFactory,
            IValueUpdateManager valueUpdateManager,
            IOptions<HardwareMonitorParameter> hardwareMonitorParameter)
        {
            OpenHardwareMonitorLib.Logging.LoggerFactory = loggerFactory;

            _logger = loggerFactory.CreateLogger<HardwareMonitor>();
            _valueUpdateManager = valueUpdateManager;
            _hardwareMonitorParameter = hardwareMonitorParameter.Value;
            _stopwatch = new Stopwatch();
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
            if (_isDisposed)
                return;

            try
            {
                _task.Wait();
                StopToMonitor();

                _computer.Close();
            }
            catch
            { }
            finally
            {
                _task = null;
                _isDisposed = true;
            }
        }

        ISensorInfo ISensorFinder.Find(string id)
        {
            static ISensor searchSensor(string id, IHardware[] hardwares)
            {
                foreach (var hardware in hardwares)
                {
                    var sub = hardware.SubHardware;
                    if (sub is not null)
                    {
                        var result = searchSensor(id, sub);
                        if (result is not null)
                            return result;
                    }

                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Identifier.ToString() == id)
                            return sensor;
                    }
                }

                return null;
            }

            WaitToInitializeAsync().Wait();

            return _sensorDictionary.Values.FirstOrDefault(x => x.Id == id);
        }

        IHardwareInfo IHardwareFinder.Find(string id)
        {
            static IHardware searchHardware(string id, IHardware[] hardwares)
            {
                foreach (var hardware in hardwares)
                {
                    if (hardware.Identifier.ToString() == id)
                        return hardware;

                    var sub = hardware.SubHardware;
                    if (sub is not null)
                    {
                        var result = searchHardware(id, sub);
                        if (result is not null)
                            return result;
                    }
                }

                return null;
            }

            WaitToInitializeAsync().Wait();

            var h  = searchHardware(id, _computer.Hardware);
            return h is not null ? new HardwareInfo(h) : null;
        }

        public void StartToInitialize()
        {
            ThrowExceptionIfDisposed();

            if (_isInitialized)
                return;

            _task = Task.Run(() =>
            {
                _computer.Open();

                UpdateHardware(_computer.Hardware);
                AddSensorCollection(_computer.Hardware);

                StartToMonitor();

                _logger.LogInformation("Complete to initialized.");

                _isInitialized = true;
            });
        }

        public async Task WaitToInitializeAsync()
        {
            ThrowExceptionIfDisposed();

            if (_task == null)
                StartToInitialize();

            if (_task.IsCompleted)
                return;

            await _task.WaitAsync(default(CancellationToken));
        }

        public async Task<IMonitorTarget[]> GetMonitorTargetsAsync(params MonitorTargetType[] types)
        {
            ThrowExceptionIfDisposed();

            await WaitToInitializeAsync();

            return _computer.Hardware
                .Select(x => new MonitorTarget(x, types))
                .ToArray();
        }

        private void StartToMonitor()
        {
            if (_valueUpdateManagerId is not null)
                return;

            _valueUpdateManagerId = _valueUpdateManager.Register(
                nameof(HardwareMonitor),
                _hardwareMonitorParameter.Interval,
                UpdateHardware);
        }

        private void StopToMonitor()
        {
            if (_valueUpdateManagerId is null)
                return;

            _valueUpdateManager.Unregister(_valueUpdateManagerId);
        }

        private void UpdateHardware()
        {
            _stopwatch.Restart();
            var start = _stopwatch.Elapsed;

            UpdateHardware(_computer.Hardware);
            UpdateSensorValues(_computer.Hardware);

            var elapsed = _stopwatch.Elapsed - start;

            _logger.LogTrace("Sensors updated. time:{ms}ms", elapsed.TotalMilliseconds);
        }

        private static void UpdateHardware(IHardware[] hardwares)
        {
            foreach (var hardware in hardwares)
            {
                hardware.Update();
                var sub = hardware.SubHardware;
                if (sub is not null)
                    UpdateHardware(sub);
            }
        }

        private void AddSensorCollection(IHardware[] hardwares)
        {
            foreach (var hardware in hardwares)
            {
                var sub = hardware.SubHardware;
                if (sub is not null)
                    AddSensorCollection(sub);

                foreach (var sensor in hardware.Sensors)
                {
                    var id = sensor.Identifier;
                    if (!_sensorDictionary.ContainsKey(id))
                    {
                        _sensorDictionary.Add(id, new SensorInfo(sensor));
                    }
                }
            }
        }

        private void UpdateSensorValues(IHardware[] hardwares)
        {
            foreach (var hardware in hardwares)
            {
                var sub = hardware.SubHardware;
                if (sub is not null)
                    UpdateSensorValues(sub);

                foreach (var sensor in hardware.Sensors)
                {
                    var id = sensor.Identifier;
                    if (_sensorDictionary.ContainsKey(id))
                    {
                        _sensorDictionary[id].UpdateValue(sensor);
                    }
                    else
                    {
                        // sensor added after InitializeAsync()
                        _sensorDictionary.Add(id, new SensorInfo(sensor));
                    }
                }
            }
        }

        private void ThrowExceptionIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("");
        }

        private class MonitorTarget : IMonitorTarget
        {
            public string Id { get; }
            public string Name { get; }
            public string Value { get; }
            public MonitorTargetType Type { get; }
            public IReadOnlyCollection<IMonitorTarget> Children { get; }

            public MonitorTarget(IHardware hardware, MonitorTargetType[] types)
            {
                if (!types.Contains(MonitorTargetType.Hardware))
                    throw new InvalidOperationException();

                var childrens = new List<MonitorTarget>();
                foreach (var h in hardware.SubHardware)
                {
                    childrens.Add(new MonitorTarget(h, types));
                }
                if (types.Contains(MonitorTargetType.Sensor))
                {
                    foreach (var s in hardware.Sensors)
                    {
                        childrens.Add(new MonitorTarget(s));
                    }
                }

                Id = hardware.Identifier.ToString();
                Name = hardware.Name;
                Value = null;
                Type = MonitorTargetType.Hardware;
                Children = childrens;
            }

            private MonitorTarget(ISensor sensor)
            {
                Id = sensor.Identifier.ToString();
                Name = sensor.Name;
                Value = GetValueStr(sensor.SensorType, sensor.Value);
                Type = MonitorTargetType.Sensor;
                Children = Array.Empty<MonitorTarget>();
            }

            private string GetValueStr(SensorType sensorType, double? value)
            {
                if (!value.HasValue)
                    return null;

                var v = value.Value;
                return sensorType switch
                {
                    SensorType.Voltage => $"{v:F1} V",
                    SensorType.Load => $"{v:F1} %",
                    SensorType.Fan => $"{v:F1} RPM",
                    SensorType.Flow => $"{v:F1} L/h",
                    SensorType.Control => $"{v:F1} %",
                    SensorType.Level => $"{v:F1} %",
                    SensorType.Power => $"{v:F1} W",
                    SensorType.RawValue => $"{v}",
                    SensorType.Factor => $"{v}",
                    SensorType.TimeSpan => v.Seconds().Humanize(culture: new("en-us")),
                    SensorType.Data => v.Gigabytes().Humanize(),
                    SensorType.SmallData => v.Megabytes().Humanize(),
                    SensorType.Throughput => v.Megabytes().Per(TimeSpan.FromSeconds(1)).Humanize(),
                    SensorType.Temperature => ValueToStringHelper.CelsiusToThermalString(v, ThermalUnit.Celsius, "F1", true),
                    SensorType.Clock => ValueToStringHelper.ToFrequencyString(v * 1024 * 1024, "F1", true),
                    _ => throw new InvalidOperationException(),
                };
            }
        }
    }
}
