using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.Buffered;
using DotNetCI.Engine;
using DotNetCI.Extensions;
using static CliWrap.Cli;

namespace DotNetCI.Tool.CiJobs
{
    class UnitTestsJob : ICiJob
    {
        private readonly CancellationToken _cancellationToken;
        private readonly string _dotnetPath;
        private readonly string _definition;
        private readonly string _configuration;
        private readonly string _filter;

        public string Name => "unit_test";
        public string? Description => "Unit tests";

        public UnitTestsJob(string dotnetPath, string definition, IEnumerable<string>? filter = null, string configuration = "Release", CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _dotnetPath = dotnetPath;
            _definition = definition;
            _configuration = configuration;
            _filter = filter?.StringJoinWithPrefix("|", "--filter ") ?? string.Empty;
        }

        public async Task ExecuteJobAsync()
        {
            string resultsDirectory = Path.GetFullPath(Path.Combine("artifacts", "tests", "unit", "output"));
            if (!Directory.Exists(resultsDirectory))
            {
                Directory.CreateDirectory(resultsDirectory);
            }
            BufferedCommandResult cmd = await Wrap(_dotnetPath)
                .WithArguments(
                $"test {_definition}" +
                _filter + " " +
                "--nologo " +
                "--no-restore " +
                $"--collect:\"XPlat Code Coverage\" --results-directory {resultsDirectory} " +
                $"--logger trx;LogFileName=\"{Path.Combine(resultsDirectory, "tests.trx").Replace("\"", "\\\"")}\" " +
                $"-c {_configuration} " +
                "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=json,cobertura"
                )
                .ToConsole()
                .ExecuteBufferedAsync(_cancellationToken).Task.ConfigureAwait(false);

            MoveAttachmentsToResultsDirectory(resultsDirectory, cmd.StandardOutput);
            TryRemoveTestsOutputDirectories(resultsDirectory);

            // Removes all files in inner folders, workaround of https://github.com/microsoft/vstest/issues/2334
            static void TryRemoveTestsOutputDirectories(string resultsDirectory)
            {
                foreach (string directory in Directory.EnumerateDirectories(resultsDirectory))
                {
                    try
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                    catch { }
                }
            }

            // Removes guid from tests output path, workaround of https://github.com/microsoft/vstest/issues/2378
            static void MoveAttachmentsToResultsDirectory(string resultsDirectory, string output)
            {
                Regex attachmentsRegex = new Regex(
                    $@"Attachments:(?<filepaths>(?<filepath>[\s]+[^\n]+{Regex.Escape(resultsDirectory)}[^\n]+[\n])+)",
                    RegexOptions.Singleline | RegexOptions.CultureInvariant);
                Match match = attachmentsRegex.Match(output);
                if (match.Success)
                {
                    string regexPaths = match.Groups["filepaths"].Value.Trim('\n', ' ', '\t', '\r');
                    string[] paths = regexPaths.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    if (paths.Length > 0)
                    {
                        foreach (string path in paths)
                        {
                            File.Move(path, Path.Combine(resultsDirectory, Path.GetFileName(path)), overwrite: true);
                        }
                        Directory.Delete(Path.GetDirectoryName(paths[0]), true);
                    }
                }
            }
        }
    }
}
