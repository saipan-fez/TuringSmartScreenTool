using Microsoft.Extensions.Logging;
using OpenCvSharp;
using TuringSmartScreenTool.Controllers;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public class ControlScreenDeviceUseCase : IControlScreenDeviceUseCase
    {
        private readonly ILogger<ControlScreenDeviceUseCase> _logger;
        private readonly IScreenDeviceManager _deviceController;

        public ControlScreenDeviceUseCase(
            ILogger<ControlScreenDeviceUseCase> logger,
            IScreenDeviceManager deviceController)
        {
            _logger = logger;
            _deviceController = deviceController;
        }

        public void Connect(ScreenDevice screenDevice, OrientationType orientation)
        {
            _deviceController.Open(screenDevice, orientation);
        }

        public void Disconnect(ScreenDevice screenDevice)
        {
            _deviceController.Close(screenDevice);
        }

        public Size GetScreenSize(ScreenDevice screenDevice)
        {
            return _deviceController.GetScreenSize(screenDevice);
        }

        public OrientationType GetOrientation(ScreenDevice screenDevice)
        {
            return _deviceController.GetOrientation(screenDevice);
        }

        public void SetOrientation(ScreenDevice screenDevice, OrientationType orientation)
        {
            _deviceController.SetOrientation(screenDevice, orientation);
        }

        public (double value, Capabilities capabilities) GetBrightness(ScreenDevice screenDevice)
        {
            return _deviceController.GetBrightness(screenDevice);
        }

        public void SetBrightness(ScreenDevice screenDevice, double value)
        {
            _deviceController.SetBrightness(screenDevice, value);
        }

        public bool IsScreenTurnedOn(ScreenDevice screenDevice)
        {
            return _deviceController.IsScreenTurnedOn(screenDevice);
        }

        public void TurnOnScreen(ScreenDevice screenDevice)
        {
            _deviceController.TurnOnScreen(screenDevice);
        }

        public void TurnOffScreen(ScreenDevice screenDevice)
        {
            _deviceController.TurnOffScreen(screenDevice);
        }
    }
}
