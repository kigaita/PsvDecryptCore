using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PsvDecryptCore.Common;
using PsvDecryptCore.Models;
using PsvDecryptCore.Services;

namespace PsvDecryptCore
{
    public class DecryptUtility
    {
        private readonly LoggingService _loggingService;
        private readonly PsvInformation _psvInformation;

        public DecryptUtility(PsvInformation psvInformation,
            LoggingService loggingService)
        {
            _psvInformation = psvInformation;
            _loggingService = loggingService;
        }

        public async Task BeginDecryptionAsync()
        {
            var sw = Stopwatch.StartNew();
            var taskQueue = new List<Task>();
            using (var psvContext = new PsvContext(_psvInformation))
            {
                foreach (var course in psvContext.Courses)
                {
                    await _loggingService.LogAsync(LogLevel.Information, $"Processing course \"{course.Name}\"")
                        .ConfigureAwait(false);
                    // Checks
                    string courseSource = Path.Combine(_psvInformation.CoursesPath, course.Name);
                    string courseOutput = Path.Combine(_psvInformation.Output, course.Title);
                    if (!Directory.Exists(courseSource))
                    {
                        await _loggingService.LogAsync(LogLevel.Warning,
                            $"Courses directory for \"{course.Name}\" not found. Skipping...").ConfigureAwait(false);
                        continue;
                    }
                    if (!Directory.Exists(courseOutput)) Directory.CreateDirectory(courseOutput);
                    // Course image copy
                    taskQueue.Add(CopyCourseImageAsync(courseSource, courseOutput));
                    // Write course info as JSON
                    taskQueue.Add(WriteCourseInfoAsync(course, courseOutput));
                    // Process each module
                    taskQueue.AddRange(await ProcessCourseModulesAsync(course, courseSource, courseOutput)
                        .ConfigureAwait(false));
                }
            }
            while (taskQueue.Any(x => !x.IsCompleted))
                try
                {
                    // Limit number of tasks per queue to prevent bottleneck.
                    Task.WaitAll(taskQueue.Where(x => !x.IsCompleted).Take(3).ToArray());
                }
                catch (AggregateException exs)
                {
                    foreach (var exception in exs.InnerExceptions)
                        await _loggingService.LogExceptionAsync(LogLevel.Error, exception).ConfigureAwait(false);
                }
            sw.Stop();
            await _loggingService.LogAsync(LogLevel.Information, $"Decryption finished after {sw.Elapsed}.")
                .ConfigureAwait(false);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Process.Start("explorer.exe", _psvInformation.Output);
        }

        private async Task<List<Task>> ProcessCourseModulesAsync(Course course, string courseSource,
            string courseOutput)
        {
            var taskQueue = new List<Task>();
            List<Module> modules;
            using (var psvContext = new PsvContext(_psvInformation))
            {
                modules = psvContext.Modules.Where(x => x.CourseName == course.Name).ToList();
            }
            await _loggingService.LogAsync(LogLevel.Information,
                $"Found {modules.Count} modules under course {course.Name}...").ConfigureAwait(false);
            foreach (var module in modules)
            {
                await _loggingService.LogAsync(LogLevel.Information, $"Processing module: {module.Name}.")
                    .ConfigureAwait(false);
                string moduleHash = await GetModuleHashAsync(module.Name, module.AuthorHandle)
                    .ConfigureAwait(false);
                string moduleOutput = Path.Combine(courseOutput,
                    $"{TitleToFileIndex(module.ModuleIndex)}. {TitleToFileName(module.Title)}");
                string moduleSource = Path.Combine(courseSource, moduleHash);
                if (!Directory.Exists(moduleOutput)) Directory.CreateDirectory(moduleOutput);
                taskQueue.Add(WriteModuleInfoAsync(module, moduleOutput));

                List<Clip> clips;
                using (var psvContext = new PsvContext(_psvInformation))
                {
                    clips = psvContext.Clips.Where(x => x.ModuleId == module.Id).ToList();
                }
                if (clips.Count == 0)
                {
                    await _loggingService.LogAsync(LogLevel.Warning,
                            $"No corresponding clips found for module {module.Name}, skipping...")
                        .ConfigureAwait(false);
                    continue;
                }
                taskQueue.Add(WriteClipInfoAsync(clips, moduleOutput));
                taskQueue.Add(Task.Run(() =>
                {
                    foreach (var clip in clips)
                    {
                        string clipSource = Path.Combine(moduleSource, $"{clip.Name}.psv");
                        string clipName = $"{TitleToFileIndex(clip.ClipIndex)}. {TitleToFileName(clip.Title)}";
                        string clipFilePath = Path.Combine(moduleOutput, $"{clipName}.mp4");
                        taskQueue.Add(DecryptFileAsync(clipSource, clipFilePath));
                        using (var psvContext = new PsvContext(_psvInformation))
                        {
                            var transcripts = psvContext.ClipTranscripts.Where(x => x.ClipId == clip.Id)
                                .ToList();
                            taskQueue.Add(BuildSubtitlesAsync(transcripts, moduleOutput, clipName));
                        }
                    }
                }));
            }

            return taskQueue;
        }

        private async Task BuildSubtitlesAsync(IList<ClipTranscript> transcripts, string srtOutput,
            string srtName)
        {
            if (!transcripts.Any()) return;
            var transcriptBuilder = new StringBuilder();
            string transcriptFileOutput = Path.Combine(srtOutput, $"{srtName}.srt");
            int lineCount = 0;
            foreach (var transcript in transcripts)
            {
                lineCount++;
                transcriptBuilder.AppendLine(lineCount.ToString());
                string startTime = TimeSpan.FromMilliseconds(transcript.StartTime).ToString(@"hh\:mm\:ss");
                string endTime = TimeSpan.FromMilliseconds(transcript.EndTime).ToString(@"hh\:mm\:ss");
                transcriptBuilder.AppendLine($"{startTime},{transcript.StartTime % 1000}" +
                                             " --> " +
                                             $"{endTime},{transcript.EndTime % 1000}");
                transcriptBuilder.AppendLine(string.Join("\n",
                    transcript.Text.Replace("\r", "").Split('\n').Select(x => "- " + x)));
                transcriptBuilder.AppendLine();
            }
            await File.WriteAllTextAsync(transcriptFileOutput, transcriptBuilder.ToString()).ConfigureAwait(false);
            await _loggingService.LogAsync(LogLevel.Debug, $"Saved {srtName} subtitles...").ConfigureAwait(false);
        }

        private static Task<string> GetModuleHashAsync(string name, string authorHandle)
        {
            string s = name + "|" + authorHandle;
            using (var md5 = MD5.Create())
            {
                return Task.FromResult(Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(s)))
                    .Replace('/', '_'));
            }
        }

        private async Task DecryptFileAsync(string srcFile, string destFile)
        {
            if (string.IsNullOrWhiteSpace(srcFile) || !File.Exists(srcFile))
            {
                await _loggingService.LogAsync(LogLevel.Warning, "Invalid source file {srcFile}, skipping...")
                    .ConfigureAwait(false);
                return;
            }

            using (var stream = new VirtualFileStream(srcFile))
            using (var output = new FileStream(destFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                output.SetLength(0);
                var buffer = stream.ReadAll();
                output.Write(buffer, 0, buffer.Length);
                await _loggingService.LogAsync(LogLevel.Information, $"Decrypted clip and saved to {destFile}.")
                    .ConfigureAwait(false);
            }
        }

        private static string TitleToFileIndex(int index)
            => index.ToString().PadLeft(2, '0');

        private static string TitleToFileName(string title)
        {
            var sb = new StringBuilder();
            foreach (char c in title)
            {
                switch (c)
                {
                    case ' ':
                        sb.Append('-');
                        break;
                    case '-':
                    case '_':
                        sb.Append('-');
                        break;
                    default:
                        if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9')
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private async Task CopyCourseImageAsync(string courseSource, string courseOutput)
        {
            string imageSrc = Path.Combine(courseSource, "image.jpg");
            string imageOutput = Path.Combine(courseOutput, "image.jpg");
            if (!File.Exists(imageSrc))
            {
                await _loggingService.LogAsync(LogLevel.Warning, $"No course image found in {courseSource}, skipping.")
                    .ConfigureAwait(false);
                return;
            }
            if (!File.Exists(imageOutput))
            {
                File.Copy(imageSrc, imageOutput);
                await _loggingService.LogAsync(LogLevel.Debug, $"Copied course image to {imageOutput}.")
                    .ConfigureAwait(false);
            }
        }

        private async Task WriteCourseInfoAsync(Course courseInfo, string courseOutput)
        {
            string serializedOutput = JsonConvert.SerializeObject(courseInfo, Formatting.Indented);
            string output = Path.Combine(courseOutput, "course-info.json");
            if (!string.IsNullOrEmpty(serializedOutput))
            {
                await File.WriteAllTextAsync(output, serializedOutput).ConfigureAwait(false);
                await _loggingService.LogAsync(LogLevel.Debug,
                    $"Finished writing course info for {courseInfo.Name}...").ConfigureAwait(false);
                return;
            }
            await _loggingService.LogAsync(LogLevel.Warning, "Invalid course info, skipping...").ConfigureAwait(false);
        }

        private async Task WriteModuleInfoAsync(Module moduleInfo, string moduleOutput)
        {
            string serializedOutput = JsonConvert.SerializeObject(moduleInfo, Formatting.Indented);
            string output = Path.Combine(moduleOutput, "module-info.json");
            if (!string.IsNullOrEmpty(serializedOutput))
            {
                await File.WriteAllTextAsync(output, serializedOutput).ConfigureAwait(false);
                await _loggingService.LogAsync(LogLevel.Debug,
                    $"Finished writing module info for {moduleInfo.Name}...").ConfigureAwait(false);
                return;
            }
            await _loggingService.LogAsync(LogLevel.Warning, "Invalid module info, skipping...").ConfigureAwait(false);
        }

        private async Task WriteClipInfoAsync(IEnumerable<Clip> clipInfo, string clipOutput)
        {
            string serializedOutput = JsonConvert.SerializeObject(clipInfo, Formatting.Indented);
            string output = Path.Combine(clipOutput, "clip-info.json");
            if (!string.IsNullOrEmpty(serializedOutput))
            {
                await File.WriteAllTextAsync(output, serializedOutput).ConfigureAwait(false);
                await _loggingService.LogAsync(LogLevel.Debug,
                    $"Finished writing clip info for {clipInfo.FirstOrDefault().Name}...").ConfigureAwait(false);
                return;
            }
            await _loggingService.LogAsync(LogLevel.Warning, "Invalid clip info, skipping...").ConfigureAwait(false);
        }
    }
}