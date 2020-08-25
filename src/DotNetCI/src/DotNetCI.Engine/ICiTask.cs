using System.Threading.Tasks;

namespace DotNetCI.Engine
{
    public interface ICiTask
    {
        string Name { get; }
        string Descrioption { get; }
        JobsCollection Jobs { get; }

        ICiTask AddJob<TJob>(TJob job)
            where TJob : ICiJob;

        Task ExecuteJobsAsync(params string[] jobs);
    }
}
