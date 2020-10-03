using System.Threading.Tasks;
using DotNetCI.Engine;

namespace DotNetCI.Tool
{
    internal class CiTask : ICiTask
    {
        public string Name { get; }

        public string Description { get; }

        public JobsCollection Jobs { get; }

        public CiTask(string name, string description = "")
        {
            Name = name;
            Description = description;
            Jobs = new JobsCollection();
        }

        public ICiTask AddJob<TJob>(TJob job) where TJob : ICiJob
        {
            Jobs.AddJob(job);

            return this;
        }

        public async Task ExecuteJobsAsync(params string[] jobs)
        {
            foreach (string job in jobs)
            {
                ICiJob? ciJob = Jobs[job];
                if (ciJob != null)
                {
                    await ciJob.ExecuteJobAsync();
                }
            }
        }
    }
}
