using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsvDecryptCore.Common;
using PsvDecryptCore.Services;

namespace PsvDecryptCore
{
    public class Program
    {
        private LoggingService _logger;
        private PsvInformation _psvInfo;

        private IServiceProvider _services;
        private DecryptUtility _utility;

        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        // ReSharper disable once UnusedMember.Local
        public static Task Main(string[] args) => new Program().StartAsync();

        public async Task StartAsync()
        {
            try
            {
                _services = await Initialize.ConfigureServicesAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                Environment.Exit(-1);
            }
            _logger = _services.GetRequiredService<LoggingService>();
            _psvInfo = _services.GetRequiredService<PsvInformation>();
            _utility = _services.GetRequiredService<DecryptUtility>();

            await _logger.LogAsync(LogLevel.Information, $"Psv Directory: {_psvInfo.DirectoryPath}")
                .ConfigureAwait(false);
            await _logger.LogAsync(LogLevel.Information, $"Courses Directory: {_psvInfo.CoursesPath}")
                .ConfigureAwait(false);
            await _logger.LogAsync(LogLevel.Information, $"Output: {_psvInfo.Output}").ConfigureAwait(false);

            var courseNameBuilder = new StringBuilder();
            foreach (string directory in _psvInfo.CoursesSubDirectories)
                courseNameBuilder.AppendLine(Path.GetFileName(directory));

            await _logger.LogAsync(LogLevel.Information, $"Found {_psvInfo.CoursesSubDirectories.Length} courses..." +
                                                         Environment.NewLine +
                                                         courseNameBuilder).ConfigureAwait(false);

            await _logger.LogAsync(LogLevel.Warning, "Press any key to start decryption...").ConfigureAwait(false);
            Console.ReadKey();
            await _utility.BeginDecryptionAsync().ConfigureAwait(false);
            await _logger.LogAsync(LogLevel.Information, "Press any key to exit.").ConfigureAwait(false);
            Console.ReadKey();
        }
    }
}