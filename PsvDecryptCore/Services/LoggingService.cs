using System;
using System.Threading.Tasks;
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
            .CreateLogger("Main");

        public Task LogAsync(LogLevel logLevel, string message)
        {
            _logger.Log(logLevel, 0, message, null, (s, exception) => s.ToString());
            return Task.CompletedTask;
        }

        public Task LogExceptionAsync(LogLevel logLevel, Exception ex)
        {
            _logger.Log(logLevel, 0, string.Empty, ex, (s, exception) => exception.ToString());
            return Task.CompletedTask;
        }
    }
}