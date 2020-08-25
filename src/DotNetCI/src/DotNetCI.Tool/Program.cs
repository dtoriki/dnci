using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetCI.Engine;
using static DotNetCI.Config.EniroinmentConfig;
using static DotNetCI.Config.ToolConfig;

namespace DotNetCI.Tool
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
            const string env = "ENVIROINMENT VAR: ";
            if (File.Exists(path))
            {
                Console.WriteLine($"{env}File \"{path}\" found.\nSet enviroinment variables from {path} ->");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{env}Cant find \"{path}\" file.\nSet deffault enviroinment variables ->");
                Console.ResetColor();
            }

            foreach (KeyValuePair<string, string> item in await LoadEnviroinmentVars(path))
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value);
                
                Console.WriteLine($"    {env}\"{item.Key}\"=\"{item.Value}\"  -> Done.");
            }
            stopWatch.Stop();
            Console.WriteLine($"{env}Done. Elapsed time: {((float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency).ToString("0.0000")}s.");
        }

        private static async Task<BuilderOptions> SetToolOptionsAsync(string path)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            const string tool = "TOOL OPTIONS: ";

            if (File.Exists(path))
            {
                Console.WriteLine($"{tool}File \"{path}\" found.\nSet DotNetBuildTool options from {path} ->");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"{tool}Cant find \"{path}\" file.\nSorry, command line arguments not Implement yet.");
                Console.ResetColor();
                if (_errorIfToolConfigNotExists)
                {
                    throw new Exception($"{tool}Can't configure build tool.");
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
                    Console.WriteLine($"    {tool}Can't find and set: \"{item.Key}\"  -> Done.");
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
                    Console.WriteLine($"    {tool}{item.Key} \"{item.Value}\"  -> Done.");
                }
            }
            stopWatch.Stop();
            Console.WriteLine($"{tool}Done. Elapsed time: {((float)stopWatch.ElapsedTicks / (float)Stopwatch.Frequency).ToString("0.0000")}s.");
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
    }
}
