using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using Microsoft.Extensions.Logging;
using TuringSmartScreenLibrary.Entities;
using TuringSmartScreenLibrary.Interfaces;

namespace TuringSmartScreenLibrary
{
    public class SerialDeviceFinder : ISerialDeviceFinder
    {
        private readonly ILogger<SerialDeviceFinder> _logger;

        public SerialDeviceFinder(ILogger<SerialDeviceFinder> logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<SerialDevice> Find()
        {
            var portNames = SerialPort.GetPortNames();
            _logger.LogDebug("Serial port found. {ports}", portNames);

            return portNames.Select(x => new SerialDevice(x)).ToList();
        }
    }
}
