using Bullseye;

namespace DotNetBuildTool.Engine
{
    public class BuilderTaskOptions
    {
        private const string DEFFAULT_TASK_DESCRIPTION = "DotNetBuilder Task";
        public string TaskDescription { get; set; } = DEFFAULT_TASK_DESCRIPTION;
        public bool Clear { get; set; }
        public bool DryRun { get; set; }
        public Host Host { get; set; }
        public bool Parallel { get; set; }
        public bool SkipDependencies { get; set; }
        public bool Verbose { get; set; }
        public string?[]? Targets { get; set; }
        public string?[]? Jobs { get; set; }
        public string? Configuration  { get; set; }
    }
}
