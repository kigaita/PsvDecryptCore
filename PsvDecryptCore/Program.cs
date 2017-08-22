using System;
using System.Diagnostics;
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
        private DecryptionEngine _util;

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
            _util = _services.GetRequiredService<DecryptionEngine>();

            // Informs the user whereabouts of the courses and output.
            _logger.Log(LogLevel.Information, $"Psv Directory: {_psvInfo.DirectoryPath}");
            _logger.Log(LogLevel.Information, $"Courses Directory: {_psvInfo.CoursesPath}");
            _logger.Log(LogLevel.Information, $"Output: {_psvInfo.Output}");

            var courseNameBuilder = new StringBuilder();
            foreach (string directory in _psvInfo.CoursesSubDirectories)
                courseNameBuilder.AppendLine(Path.GetFileName(directory));

            _logger.Log(LogLevel.Information, $"Found {_psvInfo.CoursesSubDirectories.Length} courses..." +
                                                    Environment.NewLine +
                                                    courseNameBuilder);

            // Ready to begin decryption.
            _logger.Log(LogLevel.Warning, "Press any key to start decryption...");
            Console.ReadKey();
            var sw = Stopwatch.StartNew();
            try
            {
                await _util.StartAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogException(LogLevel.Error, ex);
            }
            sw.Stop();
            _logger.Log(LogLevel.Information, $"Finished after {sw.Elapsed}.");
            _logger.Log(LogLevel.Information, "Press any key to exit.");
            Console.ReadKey();
        }
    }
}