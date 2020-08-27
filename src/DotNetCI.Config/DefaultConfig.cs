using System.Collections.Generic;
using Bullseye;

namespace DotNetCI.Config
{
    internal static class DefaultConfig
    {
        public static Dictionary<string, string> EnviroinmentVariables => new Dictionary<string, string>
        {
            { "DOTNET_CLI_TELEMETRY_OPTOUT", "1" },
            { "DOTNET_SVCUTIL_TELEMETRY_OPTOUT", "1" },
            { "DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1" },
            { "DOTNET_NOLOGO", "1" },
            { "POWERSHELL_TELEMETRY_OPTOUT", "1" },
            { "POWERSHELL_UPDATECHECK_OPTOUT", "1" },
            { "DOTNET_CLI_UI_LANGUAGE", "ru" },
        };
        public static RunOptions RunOptions => new RunOptions
        {
            Host = Host.Unknown,
            Verbose = false,
            SkipDependencies = false,
            Parallel = false,
            NoColor = false,
            ListTargets = false,
            ListInputs = false,
            ListDependencies = false,
            DryRun = false,
            Clear = false,
            ListTree = false
        };
    }
}
