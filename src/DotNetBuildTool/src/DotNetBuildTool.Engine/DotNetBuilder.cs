using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bullseye;
using Bullseye.Internal;
using CliWrap;
using CliWrap.Buffered;
using static Bullseye.Targets;

namespace DotNetBuildTool.Engine
{
    public class DotNetBuilder
    {
        private readonly string _dotnetPath;
        private readonly CancellationToken _cancellationToken;

        private IList<IBuilderTask> _tasks;

        private DotNetBuilder(string? dotnetPath, CancellationToken cancellationToken = default)
        {
            _options = options;
            _dotnetPath = dotnetPath ?? string.Empty;
            _cancellationToken = cancellationToken;
            _tasks = new List<IBuilderTask>();
        }

        public static async Task<DotNetBuilder> CreatBuilderAsync(BuilderTaskOptions options, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () => new DotNetBuilder(options, await TryFindDotNetExePath(), cancellationToken));
        }

        public DotNetBuilder AddTask<TTask>(TTask task)
            where TTask : IBuilderTask
        {
            _tasks.Contains(.Add(task);
            return this;
            
        }

        public async Task ExecuteTargetsAsync(string[] targets)
        {
            Options options = new Options
            {
                Clear = _options.Clear,
                DryRun = _options.DryRun,
                Host = _options.Host,
                ListDependencies = false,
                ListInputs = false,
                ListTargets = false,
                ListTree = false,
                NoColor = false,
                Parallel = _options.Parallel,
                SkipDependencies = _options.SkipDependencies,
                Verbose = _options.Verbose
            };
            await RunTargetsWithoutExitingAsync(targets, options).ConfigureAwait(false);
        }

        private void InitTargets(CancellationToken cancellationToken = default)
        {
            Target(
                "restore",
                async () =>
                {
                    bool isPublicRelease = bool.Parse(Environment.GetEnvironmentVariable("NBGV_PublicRelease") ?? "false");
                    BufferedCommandResult cmd = await Cli.Wrap(_dotnetPath).WithArguments(
                        $"msbuild {_options.FileOrFolder} -noLogo " +
                        "-t:Restore " +
                        "-p:RestoreForce=true " +
                        "-p:RestoreIgnoreFailedSources=True " +
                        $"-p:Configuration={_options.Configuration} " +
                        // for Nerdbank.GitVersioning
                        $"-p:PublicRelease={isPublicRelease} "
                        )
                        .ToConsole()
                        .ExecuteBufferedAsync(cancellationToken).Task.ConfigureAwait(false);
                });

            Target(
                "build",
                async () =>
                {
                    BufferedCommandResult cmd = await Cli.Wrap(_dotnetPath).WithArguments(
                        $"build {_options} -noLogo -c {_options.Configuration}"
                        )
                        .ToConsole()
                        .ExecuteBufferedAsync(cancellationToken).Task.ConfigureAwait(false);
                });

            Target(
                "unit_test",
                async () =>
                {
                    string resultsDirectory = Path.GetFullPath(Path.Combine("artifacts", "tests", "unit", "output"));
                    if (!Directory.Exists(resultsDirectory))
                    {
                        Directory.CreateDirectory(resultsDirectory);
                    }
                    BufferedCommandResult cmd = await Cli.Wrap(_dotnetPath)
                        .WithArguments(
                        $"test " +
                        "--filter FullyQualifiedName~Unit " +
                        "--nologo " +
                        "--no-restore " +
                        $"--collect:\"XPlat Code Coverage\" --results-directory {resultsDirectory} " +
                        $"--logger trx;LogFileName=\"{Path.Combine(resultsDirectory, "tests.trx").Replace("\"", "\\\"")}\" " +
                        $"-c {_options.Configuration} " +
                        "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=json,cobertura"
                        )
                        .ToConsole()
                        .ExecuteBufferedAsync(cancellationToken).Task.ConfigureAwait(false);

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
                });

            Target("default", () =>
            {
                Console.WriteLine("-- use \"restore\" and/or \"build\" and/or \"unit_test\" in command line.");
            });
        }

        private static async Task<string?> TryFindDotNetExePath()
        {
            string dotnet = "dotnet";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dotnet += ".exe";
            }

            ProcessModule mainModule = Process.GetCurrentProcess().MainModule;
            if (!string.IsNullOrEmpty(mainModule?.FileName)
                && Path.GetFileName(mainModule.FileName)!.Equals(dotnet, StringComparison.OrdinalIgnoreCase))
            {
                return mainModule.FileName;
            }

            string? environmentVariable = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                return Path.Combine(environmentVariable, dotnet);
            }

            string? paths = Environment.GetEnvironmentVariable("PATH");
            if (paths == null)
            {
                return null;
            }

            foreach (string path in paths.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Path.Combine(path, dotnet);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            return null;
        }
    }
}
