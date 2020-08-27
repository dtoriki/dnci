using System;
using System.Threading.Tasks;
using DotNetCI.Engine;

namespace DotNetCI.Tool.CiJobs
{
    public class DefaultJob : ICiJob
    {
        public string Name => "default";

        public string? Description => string.Empty;

        public async Task ExecuteJobAsync()
        {
            await Task.Run(() => Console.WriteLine("CI Tool launched."));
        }
    }
}
