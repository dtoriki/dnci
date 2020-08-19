using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bullseye;

using CliWrap.Buffered;
using DotNetBuildTool.Engine;

using static DotNetBuildTool.Config.EniroinmentConfig;
using static DotNetBuildTool.Config.ToolConfig;

namespace DotNetBuildTool.Tool
{
    internal static class Program
    {
        private static readonly string _enviroinmentVarsConfigFile = ".dnbuildvars";
        private static readonly string _dotnetBuildToolConfig = ".dnbuild";

        private static bool _errorIfToolConfigNotExists;

        private static async Task Main(
            string[] arguments,
            CancellationToken cancellationToken,
            bool errorIfToolConfigNotExists = false
            )
        {
            //hello_its_me
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _errorIfToolConfigNotExists = errorIfToolConfigNotExists;
            string enviroinmentVarsConfigPath = Path.Combine(Environment.CurrentDirectory, _enviroinmentVarsConfigFile);
            string buildToolConfigPath = Path.Combine(Environment.CurrentDirectory, _dotnetBuildToolConfig);
            Task< BuilderOptions> setToolOptionsTask = SetToolOptionsAsync(buildToolConfigPath);
            Task setEnvVarsTask = SetEnviroinmentVariablesAsync(enviroinmentVarsConfigPath);

            BuilderOptions options = await setToolOptionsTask;
            await setEnvVarsTask;

            await DotNetBuilder.CreatBuilderAsync(options, cancellationToken).ExecuteTargetsAsync(arguments);         
        }

        private static async Task SetEnviroinmentVariablesAsync(string path)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            const string ENV = "ENVIROINMENT VAR: ";
            if (File.Exists(path))
            {
                Console.WriteLine($"{ENV}File \"{path}\" found.\nSet enviroinment variables from {path} ->");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{ENV}Cant find \"{path}\" file.\nSet deffault enviroinment variables ->");
                Console.ResetColor();
            }

            foreach (KeyValuePair<string, string> item in await LoadEnviroinmentVars(path))
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value);
                
                Console.WriteLine($"    {ENV}\"{item.Key}\"=\"{item.Value}\"  -> Done.");
            }
            stopWatch.Stop();
            Console.WriteLine($"{ENV}Done. Elapsed time: {((float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency).ToString("0.0000")}s.");
        }

        private static async Task<BuilderOptions> SetToolOptionsAsync(string path)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            const string TOOL = "TOOL OPTIONS: ";

            if (File.Exists(path))
            {
                Console.WriteLine($"{TOOL}File \"{path}\" found.\nSet DotNetBuildTool options from {path} ->");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{TOOL}Cant find \"{path}\" file.\nSorry, command line arguments not Implement yet.");
                Console.ResetColor();
                if (_errorIfToolConfigNotExists)
                {
                    throw new Exception($"{TOOL}Can't configure build tool.");
                }
            }
            BuilderOptions result = new BuilderOptions();
            await foreach (KeyValuePair<string, string> item in LoadToolConfig(path))
            {
                PropertyInfo? property = typeof(BuilderOptions)
                    .GetProperties()
                    .FirstOrDefault(x => x.Name.ToUpper() == item.Key.ToUpper());
                if (property == null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"    {TOOL}Can't find and set: \"{item.Key}\"  -> Done.");
                    Console.ResetColor();

                }
                else
                {
                    bool isParsed = Enum.TryParse(property.PropertyType.Name.ToUpper(), out OptionType optionType);
                    if (!isParsed)
                    {
                        optionType = property.PropertyType.IsEnum ? OptionType.ENUM : OptionType.Unknown;
                    }
                    try
                    {
                        object givenObj = optionType switch
                        {
                            OptionType.BOOLEAN => ParseBool(item.Value),
                            OptionType.STRING => item.Value,
                            OptionType.ENUM => ParseEnum(property.PropertyType, item.Value),
                            _ => throw new ArgumentException($"Unknown type: {property.PropertyType}")
                        };
                        property.SetValue(result, givenObj);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"{item.Key} option can't be parsed", ex);
                    }
                    Console.WriteLine($"    {TOOL}{item.Key} \"{item.Value}\"  -> Done.");
                }
            }
            stopWatch.Stop();
            Console.WriteLine($"{TOOL}Done. Elapsed time: {((float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency).ToString("0.0000")}s.");
            return result;

            bool ParseBool(string str)
            {
                return str switch
                {
                    "0" => false,
                    "1" => true,
                    _ => throw new ArgumentException($"Can't parse value: {str}")
                };
            }

            object ParseEnum(Type enumType, string str)
            {
                return Enum.Parse(enumType, str);
            }
        }

        //string dotnet = TryFindDotNetExePath()
        //   ?? throw new FileNotFoundException("'dotnet' command isn't found. Try to set DOTNET_ROOT variable.");

        //if (info)
        //{
        //    Console.WriteLine(".Net Build Tool");
        //}
        //if ((arguments?.Count() ?? 0) == 0)
        //{
        //    Console.WriteLine("Can't find any targets for msbuild.");
        //    return;
        //}
        //SetEnvVariables();

        //var options = new Options
        //{
        //    Clear = clear,
        //    DryRun = dryRun,
        //    Host = host,
        //    ListDependencies = listDependencies,
        //    ListInputs = listInputs,
        //    ListTargets = listTargets,
        //    ListTree = listTree,
        //    NoColor = noColor,
        //    Parallel = parallel,
        //    SkipDependencies = skipDependencies,
        //    Verbose = verbose
        //};

        //Target(
        //    "restore",
        //    async () =>
        //    {
        //        bool isPublicRelease = bool.Parse(Environment.GetEnvironmentVariable("NBGV_PublicRelease") ?? "false");
        //        BufferedCommandResult cmd = await Cli.Wrap(dotnet).WithArguments($"msbuild {target} -noLogo " +
        //            "-t:Restore " +
        //            "-p:RestoreForce=true " +
        //            "-p:RestoreIgnoreFailedSources=True " +
        //            $"-p:Configuration={configuration} " +
        //            // for Nerdbank.GitVersioning
        //            $"-p:PublicRelease={isPublicRelease} "
        //            ).ToConsole()
        //            .ExecuteBufferedAsync().Task.ConfigureAwait(false);
        //    });

        //Target(
        //    "build",
        //    async () =>
        //    {
        //        BufferedCommandResult cmd = await Cli.Wrap(dotnet).WithArguments($"build {target} -noLogo -c {configuration}")
        //            .ToConsole()
        //            .ExecuteBufferedAsync().Task.ConfigureAwait(false);
        //    });

        //Target(
        //    "unit_test",
        //    async () =>
        //    {
        //        string resultsDirectory = Path.GetFullPath(Path.Combine("artifacts", "tests", "unit", "output"));
        //        if (!Directory.Exists(resultsDirectory))
        //            Directory.CreateDirectory(resultsDirectory);
        //        BufferedCommandResult cmd = await Cli.Wrap(dotnet)
        //            .WithArguments($"test " +
        //            "--filter FullyQualifiedName~Unit " +
        //            "--nologo " +
        //            "--no-restore " +
        //            $"--collect:\"XPlat Code Coverage\" --results-directory {resultsDirectory} " +
        //            $"--logger trx;LogFileName=\"{Path.Combine(resultsDirectory, "tests.trx").Replace("\"", "\\\"")}\" " +
        //            $"-c {configuration} " +
        //            "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=json,cobertura"
        //            )
        //            .ToConsole()
        //            .ExecuteBufferedAsync().Task.ConfigureAwait(false);

        //        MoveAttachmentsToResultsDirectory(resultsDirectory, cmd.StandardOutput);
        //        TryRemoveTestsOutputDirectories(resultsDirectory);

        //        // Removes all files in inner folders, workaround of https://github.com/microsoft/vstest/issues/2334
        //        static void TryRemoveTestsOutputDirectories(string resultsDirectory)
        //        {
        //            foreach (string directory in Directory.EnumerateDirectories(resultsDirectory))
        //            {
        //                try
        //                {
        //                    Directory.Delete(directory, recursive: true);
        //                }
        //                catch { }
        //            }
        //        }

        //        // Removes guid from tests output path, workaround of https://github.com/microsoft/vstest/issues/2378
        //        static void MoveAttachmentsToResultsDirectory(string resultsDirectory, string output)
        //        {
        //            Regex attachmentsRegex = new Regex($@"Attachments:(?<filepaths>(?<filepath>[\s]+[^\n]+{Regex.Escape(resultsDirectory)}[^\n]+[\n])+)", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        //            Match match = attachmentsRegex.Match(output);
        //            if (match.Success)
        //            {
        //                string regexPaths = match.Groups["filepaths"].Value.Trim('\n', ' ', '\t', '\r');
        //                string[] paths = regexPaths.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        //                if (paths.Length > 0)
        //                {
        //                    foreach (string path in paths)
        //                    {
        //                        File.Move(path, Path.Combine(resultsDirectory, Path.GetFileName(path)), overwrite: true);
        //                    }
        //                    Directory.Delete(Path.GetDirectoryName(paths[0]), true);
        //                }
        //            }
        //        }
        //    });

        //Target("default", DependsOn("build"));

        //await RunTargetsAndExitAsync(arguments, options).ConfigureAwait(false);

        //static void SetEnvVariables()
        //{
        //    Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
        //    Environment.SetEnvironmentVariable("DOTNET_SVCUTIL_TELEMETRY_OPTOUT", "1");
        //    Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
        //    Environment.SetEnvironmentVariable("DOTNET_NOLOGO", "1");
        //    Environment.SetEnvironmentVariable("POWERSHELL_TELEMETRY_OPTOUT", "1");
        //    Environment.SetEnvironmentVariable("POWERSHELL_UPDATECHECK_OPTOUT", "1");
        //    Environment.SetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "ru");
        //}

        //
        //
        //
        //
        //

        //
        //
        //

        //
        //
        //

        //
        //
        //

        //
        //
        //
        //
        //
        //
        //
        //
    }
}
