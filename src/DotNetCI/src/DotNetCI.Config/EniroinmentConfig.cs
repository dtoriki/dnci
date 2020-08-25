using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCI.Config
{
    public static class EniroinmentConfig
    {
        private static readonly Dictionary<string, string> _enviroinmentVariables = new Dictionary<string, string>()
        {
            { "DOTNET_CLI_TELEMETRY_OPTOUT", "1" },
            { "DOTNET_SVCUTIL_TELEMETRY_OPTOUT", "1" },
            { "DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1" },
            { "DOTNET_NOLOGO", "1" },
            { "POWERSHELL_TELEMETRY_OPTOUT", "1" },
            { "POWERSHELL_UPDATECHECK_OPTOUT", "1" },
            { "DOTNET_CLI_UI_LANGUAGE", "ru" },
        };

        public static async Task<Dictionary<string, string>> LoadEnviroinmentVars(string path)
        {
            Dictionary<string, string> result = _enviroinmentVariables
                .ToDictionary(x => x.Key, x => x.Value);
            if (!File.Exists(path))
            {
                return result;
            }

            await foreach (KeyValuePair<string, string> item in BaseConfig.LoadFromFile(path))
            {
                result[item.Key] = item.Value;
            }

            return result;
        }
    }
}
