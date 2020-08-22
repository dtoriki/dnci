using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBuildTool.Engine
{
    public interface IBuilderTask : IEqualityComparer<IBuilderTask>
    {
        BuilderTaskOptions TaskOptions { get; }
        string TaskName { get; }
        string[] Dependecies { get; }

        Task ExecuteTaskAsync();
        TTask AddDependency<TTask>(TTask dependency)
            where TTask : IBuilderTask;
        TTask AddDependency<TTask>(string dependency)
            where TTask : IBuilderTask;
    }
}
