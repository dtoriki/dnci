using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using DotNetBuildTool.Engine.Builder;
using DotNetBuildTool.Tool.CliWrap;

namespace DotNetBuildTool.Tool.Jobs
{
    public class RestoreJob : IJob
    {
        private readonly string _definition;
        private readonly string _configuration;
        private readonly string _dotnetPath;
        private readonly CancellationToken _cancelationToken;

        public string Name => "restore";
        public Func<Task> JobAction => () => JobExecute();

        public RestoreJob(string definition, string configuration, string dotnetPath, CancellationToken cancellationToken = default)
        {
            _definition = definition;
            _configuration = configuration;
            _cancelationToken = cancellationToken;
            _dotnetPath = dotnetPath;
        }

        private Task JobExecute()
        {
            return Task.Run(async () =>
                {
                    bool isPublicRelease = bool.Parse(Environment.GetEnvironmentVariable("NBGV_PublicRelease") ?? "false");
                    BufferedCommandResult cmd = await Cli.Wrap(_dotnetPath).WithArguments(
                        $"msbuild {_definition} -noLogo " +
                        "-t:Restore " +
                        "-p:RestoreForce=true " +
                        "-p:RestoreIgnoreFailedSources=True " +
                        $"-p:Configuration={_configuration} " +
                        // for Nerdbank.GitVersioning
                        $"-p:PublicRelease={isPublicRelease} "
                        )
                        .ToConsole()
                        .ExecuteBufferedAsync(_cancelationToken).Task.ConfigureAwait(false);
                });

        }
    }
}
