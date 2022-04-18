using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Logging
{
    internal static class ILoggerExtension
    {
        public static void LogEnteredMethod(this ILogger logger, [CallerMemberName]string name = null)
        {
            logger.LogTrace("Enter:{name}", name);
        }
    }
}
