
using System;

namespace WeatherLib
{
    public class WeatherException : Exception
    {
        public WeatherException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
