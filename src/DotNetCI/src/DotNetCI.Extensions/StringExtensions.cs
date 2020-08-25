using System;

namespace DotNetCI.Extensions
{
    public static class StringExtensions
    {
        private static readonly char[] _boundary = new[] { '_' };
        public static string ToFirstLetterUpper(this string @string)
        {
            if (string.IsNullOrEmpty(@string))
            {
                return @string;
            }
            string[] segments = @string.ToLower().Trim(_boundary).Split(_boundary, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Empty;
            foreach (string segment in segments)
            {
                result += segment.Substring(0, 1).ToUpperInvariant() + result.Substring(1);
            }
            return result;
        }
    }
}
