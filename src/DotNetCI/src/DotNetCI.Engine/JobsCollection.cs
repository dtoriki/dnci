using System.Collections;
using System.Collections.Generic;

namespace DotNetCI.Engine
{
    public class JobsCollection : IReadOnlyCollection<ICiJob>, IEnumerable<ICiJob>
    {
        private readonly ICollection<ICiJob> _jobs;

        public int Count => _jobs.Count;

        public JobsCollection()
        {
            _jobs = new List<ICiJob>();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _jobs.GetEnumerator();
        }

        public IEnumerator<ICiJob> GetEnumerator()
        {
            return _jobs.GetEnumerator();
        }

        public JobsCollection AddJob<TJob>(TJob job)
            where TJob : ICiJob
        {
            _jobs.Add(job);
            return this;
        }
    }
}
