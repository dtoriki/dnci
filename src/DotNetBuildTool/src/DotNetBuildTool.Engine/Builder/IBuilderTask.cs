using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBuildTool.Engine.Builder
{
    public interface IBuilderTask
    {
        string Description { get; }
        IEnumerable<string> Jobs { get; }
        IEnumerable<string> Depends { get; }
        IEnumerable<string> Definitions { get; }
    }
}
