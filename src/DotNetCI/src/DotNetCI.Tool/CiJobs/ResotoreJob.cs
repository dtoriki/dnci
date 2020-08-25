using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap.Buffered;
using DotNetCI.Engine;
using DotNetCI.Extensions;
using static CliWrap.Cli;

namespace DotNetCI.Tool.CiJobs
{
    public class ResotoreJob : ICiJob
    {
        private readonly CancellationToken _cancellationToken;
        private readonly string _dotnetPath;
        private readonly string _definition;
        private readonly string _configuration;

        public string Name => "restore";
        public string Description => "Nuget restore tool";

        public ResotoreJob(string dotnetPath, string definition, string configuration = "Release", CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            _dotnetPath = dotnetPath;
            _definition = definition;
            _configuration = configuration;
        }

        public async Task ExecuteJobAsync()
        {
            bool isPublicRelease = bool.Parse(Environment.GetEnvironmentVariable("NBGV_PublicRelease") ?? "false");
            BufferedCommandResult cmd = await Wrap(_dotnetPath)
                .WithArguments(
                    $"msbuild {_definition} -noLogo " +
                    "-t:Restore " +
                    "-p:RestoreForce=true " +
                    "-p:RestoreIgnoreFailedSources=True " +
                    $"-p:Configuration={_configuration} " +
                    // for Nerdbank.GitVersioning
                    $"-p:PublicRelease={isPublicRelease} "
                )
                .ToConsole()
                .ExecuteBufferedAsync(_cancellationToken).Task.ConfigureAwait(false);
        }
    }
}
