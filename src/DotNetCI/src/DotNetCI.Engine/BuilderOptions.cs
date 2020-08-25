using Bullseye;

namespace DotNetCI.Engine
{
    public class BuilderOptions
    {
        public bool Clear { get; set; }
        public bool DryRun { get; set; }
        public Host Host { get; set; }
        public bool Parallel { get; set; }
        public bool SkipDependencies { get; set; }
        public bool Verbose { get; set; }
        public string? FileOrFolder { get; set; }
        public bool ErrorIfTargetsNotExists { get; set; }
        public bool PrintInfo { get; set; }
        public string? Configuration  { get; set; }
    }
}
