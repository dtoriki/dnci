using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetCI.Config;
using DotNetCI.Engine;
using DotNetCI.Tool.CiJobs;

namespace DotNetCI.Tool
{

    internal static class Program
    {
        private static readonly string _configFile = "dnci.yml";
        
        private static string? _dotnetPath;
        private static CancellationToken _cancellationToken;

        private static async Task<int> Main(
            string[] arguments,
            CancellationToken cancellationToken
            )
        {
            Environment.SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1");
            _cancellationToken = cancellationToken;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string configFilePath = Path.Combine(Environment.CurrentDirectory, _configFile);
            Console.WriteLine($"Try find {_configFile}\n");
            if (!File.Exists(configFilePath))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{_configFile} not found! Create and configure it on repository root!");
                return 1;
            }
            Stopwatch stopWatch = new Stopwatch();
            try
            {
                _dotnetPath = await TryFindDotNetPathAsync(stopWatch);
                if (string.IsNullOrEmpty(_dotnetPath))
                {
                    throw new FileNotFoundException("Can't find dotnet execution file.");
                }
                CIConfigRepository configRepository = await ReadConfigFileAsync(configFilePath, stopWatch);
                SetEnviroinmentVariables(configRepository.Variables, stopWatch);

                DotNetBuilder dotnetBuilder = new DotNetBuilder(configRepository.Options);
                dotnetBuilder.AddCiTaskRange(GetTasks(configRepository.Tasks));
                await dotnetBuilder.ExecuteTargetsAsync();

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Fatal error: {AggregateFatalErrorMessage(ex)}\n");
                return -1;
            }
            finally
            {
                stopWatch.Stop();
                Console.ResetColor();
                Console.WriteLine($"Elapsed time: {(float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency}");
            }

            return 0;
        }

        private static string AggregateFatalErrorMessage(Exception? ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            string result = ex.Message;

            return result.TrimEnd('.') + ". " + AggregateFatalErrorMessage(ex.InnerException);
        }

        private static async Task<CIConfigRepository> ReadConfigFileAsync(string configFilePath, Stopwatch stopwatch)
        {
            Console.WriteLine($"Read {_configFile} file...");
            stopwatch.Start();
            CIConfigRepository configRepository = await CIConfigRepository.CreateAsync(configFilePath, true);
            stopwatch.Stop();
            Console.WriteLine(GetElapsedTimeMessage(stopwatch));
            stopwatch.Reset();

            return configRepository;
        }

        private static void SetEnviroinmentVariables(Dictionary<string, string> variables, Stopwatch stopwatch)
        {
            Console.WriteLine($"Set enviroinment variables...");
            stopwatch.Start();
            foreach (KeyValuePair<string, string> @var in variables)
            {
                Console.WriteLine($"{var.Key} = {var.Value}");
                Environment.SetEnvironmentVariable(var.Key, var.Value);
            }
            stopwatch.Stop();
            Console.WriteLine(GetElapsedTimeMessage(stopwatch));
        }

        private static string GetElapsedTimeMessage(Stopwatch stopwatch)
        {
            return $"Done! Elapsed time: {(float)stopwatch.ElapsedTicks / (float)Stopwatch.Frequency}\n";
        }

        private static IEnumerable<ICiTask> GetTasks(IEnumerable<CITaskConfig> tasksConfig)
        {
            List<ICiTask> result = new List<ICiTask>();
            foreach (CITaskConfig taskConfig in tasksConfig)
            {
                ICiTask ciTask = new CiTask(taskConfig.Name, taskConfig.Description);
                foreach (CIJobConfig jobConfig in taskConfig.Jobs)
                {
                    ICiJob newJob = jobConfig.Name switch
                    {
                        "restore" => new ResotoreJob(_dotnetPath, taskConfig.Definitions, jobConfig.Configuration, _cancellationToken),
                        _ => new DefaultJob()
                    };

                    ciTask.AddJob(newJob);
                }

                result.Add(ciTask);
            }

            return result;
        }

        private static string? TryFindDotNetPath()
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

        private static async Task<string?> TryFindDotNetPathAsync(Stopwatch stopwatch)
        {
            Console.Write("Try find dotnet path ...");
            stopwatch.Start();
            string? dotnetPath = await Task.Run(() => TryFindDotNetPath());
            stopwatch.Stop();
            Console.WriteLine(GetElapsedTimeMessage(stopwatch));
            stopwatch.Reset();
            return dotnetPath;
        }

        //private static async Task SetEnviroinmentVariablesAsync(string path)
        //{
        //    Stopwatch stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    const string env = "ENVIROINMENT VAR: ";
        //    if (File.Exists(path))
        //    {
        //        Console.WriteLine($"{env}File \"{path}\" found.\nSet enviroinment variables from {path} ->");
        //    }
        //    else
        //    {
        //        Console.ForegroundColor = ConsoleColor.DarkYellow;
        //        Console.WriteLine($"{env}Cant find \"{path}\" file.\nSet deffault enviroinment variables ->");
        //        Console.ResetColor();
        //    }

        //    foreach (KeyValuePair<string, string> item in await LoadEnviroinmentVars(path))
        //    {
        //        Environment.SetEnvironmentVariable(item.Key, item.Value);

        //        Console.WriteLine($"    {env}\"{item.Key}\"=\"{item.Value}\"  -> Done.");
        //    }
        //    stopWatch.Stop();
        //    Console.WriteLine($"{env}Done. Elapsed time: {((float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency).ToString("0.0000")}s.");
        //}

        //private static async Task<BuilderOptions> SetToolOptionsAsync(string path)
        //{
        //    Stopwatch stopWatch = new Stopwatch();
        //    stopWatch.Start();
        //    const string tool = "TOOL OPTIONS: ";

        //    if (File.Exists(path))
        //    {
        //        Console.WriteLine($"{tool}File \"{path}\" found.\nSet DotNetBuildTool options from {path} ->");
        //    }
        //    else
        //    {
        //        Console.ForegroundColor = ConsoleColor.DarkRed;
        //        Console.WriteLine($"{tool}Cant find \"{path}\" file.\nSorry, command line arguments not Implement yet.");
        //        Console.ResetColor();
        //        if (_errorIfToolConfigNotExists)
        //        {
        //            throw new Exception($"{tool}Can't configure build tool.");
        //        }
        //    }
        //    BuilderOptions result = new BuilderOptions();
        //    await foreach (KeyValuePair<string, string> item in LoadToolConfig(path))
        //    {
        //        PropertyInfo? property = typeof(BuilderOptions)
        //            .GetProperties()
        //            .FirstOrDefault(x => x.Name.ToUpper() == item.Key.ToUpper());
        //        if (property == null)
        //        {
        //            Console.ForegroundColor = ConsoleColor.DarkYellow;
        //            Console.WriteLine($"    {tool}Can't find and set: \"{item.Key}\"  -> Done.");
        //            Console.ResetColor();

        //        }
        //        else
        //        {
        //            bool isParsed = Enum.TryParse(property.PropertyType.Name.ToUpper(), out OptionType optionType);
        //            if (!isParsed)
        //            {
        //                optionType = property.PropertyType.IsEnum ? OptionType.ENUM : OptionType.Unknown;
        //            }
        //            try
        //            {
        //                object givenObj = optionType switch
        //                {
        //                    OptionType.BOOLEAN => ParseBool(item.Value),
        //                    OptionType.STRING => item.Value,
        //                    OptionType.ENUM => ParseEnum(property.PropertyType, item.Value),
        //                    _ => throw new ArgumentException($"Unknown type: {property.PropertyType}")
        //                };
        //                property.SetValue(result, givenObj);
        //            }
        //            catch (ArgumentException ex)
        //            {
        //                throw new ArgumentException($"{item.Key} option can't be parsed", ex);
        //            }
        //            Console.WriteLine($"    {tool}{item.Key} \"{item.Value}\"  -> Done.");
        //        }
        //    }
        //    stopWatch.Stop();
        //    Console.WriteLine($"{tool}Done. Elapsed time: {((float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency).ToString("0.0000")}s.");
        //    return result;

        //    bool ParseBool(string str)
        //    {
        //        return str switch
        //        {
        //            "0" => false,
        //            "1" => true,
        //            _ => throw new ArgumentException($"Can't parse value: {str}")
        //        };
        //    }

        //    object ParseEnum(Type enumType, string str)
        //    {
        //        return Enum.Parse(enumType, str);
        //    }
        //}
    }
}
