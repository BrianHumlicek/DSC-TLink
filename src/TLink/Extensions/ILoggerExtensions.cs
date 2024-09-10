using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DSC.TLink.Extensions
{
    internal static class ILoggerExtensions
    {
        public static void LogDebug(this ILogger log, Func<string> message)
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug(message());
            }
        }
        public static void LogTrace(this ILogger log, Func<string> message)
        {
            if (log.IsEnabled(LogLevel.Trace))
            {
                log.LogTrace(message());
            }
        }
    }
}
