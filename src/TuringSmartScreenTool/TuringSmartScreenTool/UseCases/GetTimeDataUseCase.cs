using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.UseCases
{
    public class GetTimeDataUseCase : IGetTimeDataUseCase
    {
        private readonly ILogger<GetTimeDataUseCase> _logger;
        private readonly ITimeManager _timeManager;

        public GetTimeDataUseCase(
            ILogger<GetTimeDataUseCase> logger,
            ITimeManager timeManager)
        {
            _logger = logger;
            _timeManager = timeManager;
        }

        public ITimeData Get()
        {
            return _timeManager.Get();
        }
    }
}
