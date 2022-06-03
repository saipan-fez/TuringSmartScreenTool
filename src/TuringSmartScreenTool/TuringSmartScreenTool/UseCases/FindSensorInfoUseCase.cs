using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.UseCases
{
    public class FindSensorInfoUseCase : IFindSensorInfoUseCase
    {
        private readonly ILogger<FindHardwareInfoUseCase> _logger;
        private readonly ISensorFinder _sensorFinder;

        public FindSensorInfoUseCase(
            ILogger<FindHardwareInfoUseCase> logger,
            ISensorFinder sensorFinder)
        {
            _logger = logger;
            _sensorFinder = sensorFinder;
        }

        public ISensorInfo Find(string id)
        {
            return _sensorFinder.Find(id);
        }
    }
}
