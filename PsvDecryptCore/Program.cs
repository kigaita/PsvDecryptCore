using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsvDecryptCore.Models;
using PsvDecryptCore.Services;

namespace PsvDecryptCore
{
    public class Program
    {
        private LoggingService _logger;
        private PsvInformation _psvInfo;
        private IServiceProvider _services;
        private DecryptionModule _util;

        public static Task Main(string[] args) => new Program().StartAsync();

        /// <summary>
        ///     Main entry point.
        /// </summary>
        public async Task StartAsync()
        {
            // Preps the required services and information.
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
            _util = _services.GetRequiredService<DecryptionModule>();

            // Informs the user whereabouts of the courses and output.
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

            // Ready to begin decryption.
            await _logger.LogAsync(LogLevel.Warning, "Press any key to start decryption...").ConfigureAwait(false);
            Console.ReadKey();
            await _util.BeginDecryptionAsync().ConfigureAwait(false);
            await _logger.LogAsync(LogLevel.Information, "Press any key to exit.").ConfigureAwait(false);
            Console.ReadKey();
        }
    }
}