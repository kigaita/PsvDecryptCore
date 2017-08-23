using System;
using Microsoft.Extensions.Logging;

namespace PsvDecryptCore.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;

        public LoggingService(ILoggerFactory logger) => _logger = logger
#if DEBUG
            .AddConsole(LogLevel.Trace)
#else
            .AddConsole(LogLevel.Information)
#endif
            .AddFile($"log/{DateTime.Now:MM-dd-yy}.log")
            .CreateLogger("Main");

        public void Log(LogLevel logLevel, string message) => _logger.Log(logLevel, 0, message, null,
            (s, exception) => s.ToString());

        public void LogException(LogLevel logLevel, Exception ex) => _logger.Log(logLevel, 0, string.Empty, ex,
            (s, exception) => exception.ToString());
    }
}