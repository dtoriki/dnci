using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetBuildTool.Config
{
    internal static class BaseConfig
    {
        public static async IAsyncEnumerable<KeyValuePair<string, string>> LoadFromFile(string path, char separator = '=')
        {
            IList<string?> lines = new List<string?>();

            using (Stream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        lines.Add(await sr.ReadLineAsync());
                    }

                    foreach (string? line in lines)
                    {
                        string[] linePair = line?.Split(separator, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

                        if (linePair.Count() == 2)
                        {
                            yield return new KeyValuePair<string, string>(linePair[0].Trim(), linePair[1].Trim());
                        }
                    }
                }
            }
        }
    }
}
