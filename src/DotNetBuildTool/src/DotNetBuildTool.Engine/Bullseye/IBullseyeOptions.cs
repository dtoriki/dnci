using Bullseye;

namespace DotNetBuildTool.Engine.Bullseye
{
    public interface IBullseyeOptions
    {
        Host Host { get; }
        bool Verbose { get; }
        bool SkipDependencies { get; }
        bool Parallel { get; }
        bool NoColor { get; }
        bool ListTargets { get; }
        bool ListInputs { get; }
        bool ListDependencies { get; }
        bool DryRun { get; }
        bool Clear { get; }
        bool ListTree { get; }
    }
}
