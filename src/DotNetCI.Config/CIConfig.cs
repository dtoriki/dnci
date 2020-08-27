using System.Collections.Generic;

namespace DotNetCI.Config
{
    internal class CIConfig
    {
        public IEnumerable<string> Variables { get; set; } = new List<string>();
        public RunOptions RunOptions { get; set; } = new RunOptions();
        public IEnumerable<CITaskConfig> Tasks { get; set; } = new List<CITaskConfig>();
    }
}
