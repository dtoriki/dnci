using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bullseye;
using DotNetCI.Extensions;
using SharpYaml.Serialization;

namespace DotNetCI.Config
{
    public class CIConfigRepository
    {
        private const char SEPARATOR = ' ';

        public Dictionary<string, string> Variables { get; }
        public IEnumerable<CITaskConfig> Tasks { get; }
        public Options Options { get; }

        private CIConfigRepository(Dictionary<string, string> envVars, IEnumerable<CITaskConfig> tasks, Options options)
        {
            Variables = envVars;
            Tasks = tasks;
            Options = options;
        }

        public static async Task<CIConfigRepository> CreateAsync(string sourcePath, bool throwsExceptions = true)
        {
            CIConfig config = await ReadCIConfigFileAsync(sourcePath, throwsExceptions);
            Dictionary<string, string> enviroinmentVariables = ParseEnviroinmentVariables(config.Variables, throwsExceptions);
            Options options = GetOptions(config.RunOptions);

            return new CIConfigRepository(enviroinmentVariables, config.Tasks, options);
        }

        private static Options GetOptions(RunOptions options)
        {
            return DefaultConfig
                .RunOptions
                .MutableClone(x =>
                {
                    x.Clear = options.Clear ?? x.Clear;
                    x.DryRun = options.DryRun ?? x.DryRun;
                    x.Host = options.Host ?? x.Host;
                    x.ListDependencies = options.ListDependencies ?? x.ListDependencies;
                    x.ListInputs = options.ListInputs ?? x.ListInputs;
                    x.ListTargets = options.ListTargets ?? x.ListTargets;
                    x.ListTree = options.ListTree ?? x.ListTree;
                    x.NoColor = options.NoColor ?? x.NoColor;
                    x.Parallel = options.Parallel ?? x.Parallel;
                    x.SkipDependencies = options.SkipDependencies ?? x.SkipDependencies;
                    x.Verbose = options.Verbose ?? x.Verbose;
                }).TransferTo(options =>
                {
                    return new Options
                    {
#pragma warning disable CS8629 // Тип значения, допускающего NULL, может быть NULL.
                        Clear = options.Clear.Value,
                        DryRun = options.DryRun.Value,
                        Host = options.Host.Value,
                        ListDependencies = options.ListDependencies.Value,
                        ListInputs = options.ListInputs.Value,
                        ListTargets = options.ListTargets.Value,
                        ListTree = options.ListTree.Value,
                        NoColor = options.NoColor.Value,
                        Parallel = options.Parallel.Value,
                        SkipDependencies = options.SkipDependencies.Value,
                        Verbose = options.Verbose.Value
#pragma warning restore CS8629 // Тип значения, допускающего NULL, может быть NULL.
                    };
                });
        }
        private static Dictionary<string, string> ParseEnviroinmentVariables(IEnumerable<string> dataSet, bool throwsExceptions)
        {
            Dictionary<string, string> result = DefaultConfig
                .EnviroinmentVariables
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (string data in dataSet)
            {
                string[] dataPair = data.Split(SEPARATOR);

                if (dataPair.Count() != 2)
                {
                    if (throwsExceptions)
                    {
                        throw new InvalidDataException($"Can't set variable {data}");
                    }
                }
                else
                {
                    result[dataPair[0]] = dataPair[1];
                }
            }

            return result;
        }

        private static async Task<CIConfig> ReadCIConfigFileAsync(string sourcePath, bool throwsExceptions)
        {
            try
            {
                string content = string.Empty;
                using (StreamReader reader = new StreamReader(sourcePath))
                {
                    content = await reader.ReadToEndAsync();
                }
                Serializer serializer = new Serializer(new SerializerSettings()
                {
                    NamingConvention = new NameConvention()
                });
                return serializer.Deserialize<CIConfig>(content);
            }
            catch
            {
                if (throwsExceptions)
                {
                    throw;
                }

                return new CIConfig();
            }
        }
    }
}
