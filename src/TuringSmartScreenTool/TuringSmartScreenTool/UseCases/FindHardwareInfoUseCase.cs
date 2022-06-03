using Microsoft.Extensions.Logging;
using TuringSmartScreenTool.Controllers.Interfaces;
using TuringSmartScreenTool.Entities;
using TuringSmartScreenTool.UseCases.Interfaces;

namespace TuringSmartScreenTool.UseCases
{
    public class FindHardwareInfoUseCase : IFindHardwareInfoUseCase
    {
        private readonly ILogger<FindHardwareInfoUseCase> _logger;
        private readonly IHardwareFinder _hardwareFinder;

        public FindHardwareInfoUseCase(
            ILogger<FindHardwareInfoUseCase> logger,
            IHardwareFinder hardwareFinder)
        {
            _logger = logger;
            _hardwareFinder = hardwareFinder;
        }

        public IHardwareInfo Find(string id)
        {
            return _hardwareFinder.Find(id);
        }
    }
}
