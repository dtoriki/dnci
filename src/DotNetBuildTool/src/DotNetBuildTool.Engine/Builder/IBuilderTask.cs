using System.Collections.Generic;

namespace DotNetBuildTool.Engine.Builder
{
    public interface IBuilderTask
    {
        string Description { get; }
        IEnumerable<IJob> Jobs { get; }
        IEnumerable<string> Definitions { get; }
    }
}
