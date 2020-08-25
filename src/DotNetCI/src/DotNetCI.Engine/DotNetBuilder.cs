using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bullseye;

namespace DotNetCI.Engine
{
    public class DotNetBuilder
    {
        private readonly Options _runOptions;
        private readonly ICollection<ICiTask> _tasks;

        public DotNetBuilder(Options runOptions)
        {
            _runOptions = runOptions;
            _tasks = new List<ICiTask>();
        }

        public DotNetBuilder AddCiTask<TCiTask>(TCiTask task)
            where TCiTask : ICiTask
        {
            _tasks.Add(task);
            return this;
        }

        public async Task ExecuteTargetsAsync()
        {
            List<ConfiguredTaskAwaitable> runTargetsTasks = new List<ConfiguredTaskAwaitable>();
            foreach (ICiTask task in _tasks)
            {
                Targets targets = new Targets();

                foreach (ICiJob job in task.Jobs)
                {
                    targets.Add(job.Name, async () => await job.ExecuteJobAsync());
                }

                runTargetsTasks.Add(targets.RunWithoutExitingAsync(task.Jobs.Select(x => x.Name), _runOptions).ConfigureAwait(false));
            }

            foreach (ConfiguredTaskAwaitable runTargetTask in runTargetsTasks)
            {
                await runTargetTask;
            }
        }
    }
}
