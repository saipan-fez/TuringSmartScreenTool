using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;

namespace TuringSmartScreenTool.UseCases
{
    public class FindScreenDeviceUseCase : IFindScreenDeviceUseCase
    {
        private readonly ILogger<FindScreenDeviceUseCase> _logger;
        private readonly IScreenDeviceManager _deviceController;

        public FindScreenDeviceUseCase(
            ILogger<FindScreenDeviceUseCase> logger,
            IScreenDeviceManager deviceController)
        {
            _logger = logger;
            _deviceController = deviceController;
        }

        public IReadOnlyCollection<ScreenDevice> Find()
        {
            return _deviceController.FindDevices();
        }
    }
}
