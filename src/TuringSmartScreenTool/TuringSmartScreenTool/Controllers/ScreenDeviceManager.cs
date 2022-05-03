using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using TuringSmartScreenLibrary.Entities;
using TuringSmartScreenLibrary.Interfaces;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.Controllers
{
    public sealed class ScreenDeviceManager : IDisposable, IScreenDeviceManager
    {
        private enum DeviceState
        {
            Opened,
            RunningScreenUpdate,
            Closed
        }

        private record DeviceData(ITuringSmartScreenDevice device, Task task, CancellationTokenSource cts)
        {
            public DeviceState State
            {
                get
                {
                    if (device is null)
                        throw new InvalidOperationException("device state is invalid.");

                    if (task is not null && cts is not null)
                    {
                        return DeviceState.RunningScreenUpdate;
                    }
                    else
                    {
                        return device.IsOpen ? DeviceState.Opened : DeviceState.Closed;
                    }
                }
            }
        }

        // TODO: optional
        private static readonly TimeSpan s_screenUpdateInterval = TimeSpan.FromMilliseconds(500);

        private readonly ILogger<string> _logger;
        private readonly ISerialDeviceFinder _serialPortDeviceFinder;
        private readonly IServiceProvider _serviceProvider;

        private Dictionary<ScreenDevice, DeviceData> _devices = new();

        public ScreenDeviceManager(
            ILogger<string> logger,
            ISerialDeviceFinder serialPortDeviceFinder,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serialPortDeviceFinder = serialPortDeviceFinder;
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
            try
            {
                foreach (var screenDevice in _devices.Keys)
                    Close(screenDevice);
            }
            catch
            {
                // nop
            }
        }

        public IReadOnlyCollection<ScreenDevice> FindDevices()
        {
            return _serialPortDeviceFinder.Find()
                .Select(x => new ScreenDevice(x.PortName))
                .ToList();
        }

        public void Open(ScreenDevice screenDevice, OrientationType orientation)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (_devices.ContainsKey(screenDevice))
            {
                throw new InvalidOperationException("device is already opened.");
            }

            var serialDevice = _serialPortDeviceFinder.Find().FirstOrDefault(x => x.PortName == screenDevice.Name);
            if (serialDevice is null)
            {
                throw new InvalidOperationException("device is not exists.");
            }

            var rotation = orientation switch
            {
                OrientationType.Portrait => Rotation.Degree0,
                OrientationType.PortraitReverse => Rotation.Degrees180,
                OrientationType.Landscape => Rotation.Degrees90,
                OrientationType.LandscapeReverse => Rotation.Degrees270,
                _ => throw new InvalidOperationException(),
            };

            var device = _serviceProvider.GetService<ITuringSmartScreenDevice>();
            device.Open(serialDevice, rotation);

            _devices.Add(screenDevice, new DeviceData(device, null, null));
        }

        public void Close(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.RunningScreenUpdate)
            {
                StopToUpdateScreen(screenDevice);
            }

            d.device.Close();
            _devices[screenDevice] = d with { cts = null, task = null };
        }

        public OrientationType GetOrientation(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            return d.device.Rotate switch
            {
                Rotation.Degree0 => OrientationType.Portrait,
                Rotation.Degrees90 => OrientationType.Landscape,
                Rotation.Degrees180 => OrientationType.PortraitReverse,
                Rotation.Degrees270 => OrientationType.LandscapeReverse,
                _ => throw new InvalidOperationException(),
            };
        }

        public void SetOrientation(ScreenDevice screenDevice, OrientationType orientation)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            var rotation = orientation switch
            {
                OrientationType.Portrait => Rotation.Degree0,
                OrientationType.PortraitReverse => Rotation.Degrees180,
                OrientationType.Landscape => Rotation.Degrees90,
                OrientationType.LandscapeReverse => Rotation.Degrees270,
                _ => throw new InvalidOperationException(),
            };

            d.device.SetRotation(rotation);
        }

        public Size GetScreenSize(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            return d.device.GetScreenSize();
        }

        public (double value, Entities.Capabilities capabilities) GetBrightness(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.Closed)
            {
                throw new InvalidOperationException("device is closed.");
            }

            var device = d.device;
            var cap = device.BrightnessCapabilities;
            return (
                device.Brightness,
                new Entities.Capabilities(cap.Max, cap.Min, cap.Step, cap.Default));
        }

        public void SetBrightness(ScreenDevice screenDevice, double value)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.Closed)
            {
                throw new InvalidOperationException("device is closed.");
            }

            var device = d.device;
            var min = device.BrightnessCapabilities.Min;
            var max = device.BrightnessCapabilities.Max;
            if (value < min || max < value)
            {
                throw new ArgumentOutOfRangeException($"value must be between {min} to {max}");
            }

            device.SetBrightness(value);
        }

        public bool IsScreenTurnedOn(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.Closed)
            {
                throw new InvalidOperationException("device is closed.");
            }

            return d.device.IsScreenTurnedOn;
        }

        public void TurnOnScreen(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.Closed)
            {
                throw new InvalidOperationException("device is closed.");
            }

            d.device.TurnOnScreen();
        }

        public void TurnOffScreen(ScreenDevice screenDevice)
        {
            if (screenDevice is null)
            {
                throw new ArgumentNullException(nameof(screenDevice));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.Closed)
            {
                throw new InvalidOperationException("device is closed.");
            }

            d.device.TurnOffScreen();
        }

        public void StartToUpdateScreen(ScreenDevice screenDevice, Action<Mat<Vec3b>> updateScreenAction)
        {
            if (updateScreenAction is null)
            {
                throw new ArgumentNullException(nameof(updateScreenAction));
            }

            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is DeviceState.Closed)
            {
                throw new InvalidOperationException("device is closed.");
            }
            else if (d.State is DeviceState.RunningScreenUpdate)
            {
                throw new InvalidOperationException("device is already updating screen.");
            }

            var cts = new CancellationTokenSource();
            var task = Task.Run(() => UpdateScreenProc(d.device, updateScreenAction, cts.Token));

            // Opened -> RunningScreenUpdate
            _devices[screenDevice] = d with { cts = cts, task = task };
        }

        public void StopToUpdateScreen(ScreenDevice screenDevice)
        {
            if (!_devices.TryGetValue(screenDevice, out var d))
            {
                throw new InvalidOperationException("device is not exists.");
            }

            if (d.State is not DeviceState.RunningScreenUpdate)
            {
                // not running
                return;
            }

            try
            {
                d.cts.Cancel();
                d.task.Wait();
            }
            catch
            {
                // nop
            }
            finally
            {
                try
                {
                    d.cts.Dispose();
                    d.task.Dispose();
                }
                catch
                { }

                // RunningScreenUpdate -> Opened
                _devices[screenDevice] = d with { cts = null, task = null };
            }
        }

        private void UpdateScreenProc(ITuringSmartScreenDevice device, Action<Mat<Vec3b>> updateScreenAction, CancellationToken token)
        {
            var screenSize = device.GetScreenSize();
            using var bgrMat = new Mat<Vec3b>(screenSize.Height, screenSize.Width);

            try
            {
                var stopwatch = new Stopwatch();
                while (!token.IsCancellationRequested)
                {
                    stopwatch.Restart();
                    var start = stopwatch.Elapsed;

                    // update Mat
                    updateScreenAction(bgrMat);

                    var waypoint1 = stopwatch.Elapsed;

                    // send to device
                    device.RefreshScreen(bgrMat);

                    var finish = stopwatch.Elapsed;

                    _logger.LogTrace("Update screen  time:{ms}ms", (waypoint1 - start).TotalMilliseconds);
                    _logger.LogTrace("Send to device time:{ms}ms", (finish - waypoint1).TotalMilliseconds);

                    var waitTime = s_screenUpdateInterval - (finish - start);
                    if (waitTime > TimeSpan.Zero)
                    {
                        Thread.Sleep(waitTime);
                    }
                }
            }
            catch
            {
                // TODO: error handling
            }
        }
    }
}
