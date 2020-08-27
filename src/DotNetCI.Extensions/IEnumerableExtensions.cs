using System.Collections.Generic;

namespace DotNetCI.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string StringJoinWithPrefix<T>(this IEnumerable<T> collection, string separator = "", string prefix = "")
        {
            return prefix + string.Join(separator, collection);
        }
    }
}
