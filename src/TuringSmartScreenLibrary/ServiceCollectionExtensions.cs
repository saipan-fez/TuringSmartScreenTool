using Microsoft.Extensions.DependencyInjection;
using TuringSmartScreenLibrary.Interfaces;

namespace TuringSmartScreenLibrary
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTuringSmartScreenServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<ISerialDeviceFinder, SerialDeviceFinder>()
                .AddTransient<ITuringSmartScreenDevice, TuringSmartScreenDevice>();
        }
    }
}
