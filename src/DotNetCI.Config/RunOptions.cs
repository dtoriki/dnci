using System;
using Bullseye;

namespace DotNetCI.Config
{
    internal class RunOptions : ICloneable
    {
        public Host? Host { get; set; }
        public bool? Verbose { get; set; }
        public bool? SkipDependencies { get; set; }
        public bool? Parallel { get; set; }
        public bool? NoColor { get; set; } 
        public bool? ListTargets { get; set; }
        public bool? ListInputs { get; set; }
        public bool? ListDependencies { get; set; }
        public bool? DryRun { get; set; }
        public bool? Clear { get; set; }
        public bool? ListTree { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
