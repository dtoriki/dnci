using System.Collections.Generic;
using System.IO;

namespace DotNetBuildTool.Config
{
    public static class ToolConfig
    {
        private const char SEPARATOR = ' ';
        public static async IAsyncEnumerable<KeyValuePair<string, string>> LoadToolConfig(string path)
        {
            if (!File.Exists(path))
            {
                yield break;
            }

            await foreach (KeyValuePair<string, string> item in BaseConfig.LoadFromFile(path, SEPARATOR))
            {
                yield return new KeyValuePair<string, string>(item.Key, item.Value);
            }
        }
    }
}
