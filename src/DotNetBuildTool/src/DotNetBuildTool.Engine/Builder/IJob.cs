using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBuildTool.Engine.Builder
{
    public interface IJob
    {
        string Name { get; }
        IEnumerable<string> Depends { get; }
        Func<Task> Action { get; }
    }
}
