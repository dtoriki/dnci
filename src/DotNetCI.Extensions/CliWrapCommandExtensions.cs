using System;
using System.IO;
using CliWrap;

namespace DotNetCI.Extensions
{
    public static class CliWrapCommandExtensions
    {
        internal static Stream _stdout = Console.OpenStandardOutput();
        internal static Stream _stderr = Console.OpenStandardOutput();

        public static Command ToConsole(this Command command) => command | (_stdout, _stderr);
    }
}
