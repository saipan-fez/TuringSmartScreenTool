using System;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using TuringSmartScreenTool.Controllers;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public class UpdateScreenUseCase : IUpdateScreenUseCase
    {
        private readonly ILogger<UpdateScreenUseCase> _logger;
        private readonly IScreenDeviceManager _deviceController;

        public UpdateScreenUseCase(
            ILogger<UpdateScreenUseCase> logger,
            IScreenDeviceManager deviceController)
        {
            _logger = logger;
            _deviceController = deviceController;
        }

        public void Start(ScreenDevice screenDevice, Action<Mat<Vec3b>> updateScreenAction)
        {
            _deviceController.StartToUpdateScreen(screenDevice, updateScreenAction);
        }

        public void Stop(ScreenDevice screenDevice)
        {
            _deviceController.StopToUpdateScreen(screenDevice);
        }
    }
}
