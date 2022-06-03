using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.UseCases
{
    public class GetMonitorTargetsUseCase : IGetMonitorTargetsUseCase
    {
        private readonly ILogger<GetMonitorTargetsUseCase> _logger;
        private readonly IHardwareMonitorController _hardwareMonitorController;

        public GetMonitorTargetsUseCase(
            ILogger<GetMonitorTargetsUseCase> logger,
            IHardwareMonitorController hardwareMonitorController)
        {
            _logger = logger;
            _hardwareMonitorController = hardwareMonitorController;
        }

        public async Task<IMonitorTarget[]> GetMonitorTargetsAsync(params MonitorTargetType[] types)
        {
            return await _hardwareMonitorController.GetMonitorTargetsAsync(types);
        }
    }
}
