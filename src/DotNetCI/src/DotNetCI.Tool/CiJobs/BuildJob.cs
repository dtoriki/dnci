using System.Threading;
using System.Threading.Tasks;
using CliWrap.Buffered;
using DotNetCI.Engine;
using DotNetCI.Extensions;
using static CliWrap.Cli;

namespace DotNetCI.Tool.CiJobs
{
    public class BuildJob : ICiJob
    {
        private readonly CancellationToken _cancellationToken;
        private readonly string _dotnetPath;
        private readonly string _definition;
        private readonly string _configuration;

        public string Name => "build";
        public string? Description => "Build";

        public BuildJob(string dotnetPath, string definition, string configuration = "Release", CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _dotnetPath = dotnetPath;
            _definition = definition;
            _configuration = configuration;
        }

        public async Task ExecuteJobAsync()
        {
            BufferedCommandResult cmd = await Wrap(_dotnetPath)
                .WithArguments($"build {_definition} -noLogo -c {_configuration}")
                .ToConsole()
                .ExecuteBufferedAsync(_cancellationToken).Task.ConfigureAwait(false);
        }
    }
}
