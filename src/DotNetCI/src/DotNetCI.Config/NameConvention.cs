using System;
using System.Globalization;
using DotNetCI.Extensions;
using SharpYaml.Serialization;

namespace DotNetCI.Config
{
    internal class NameConvention : IMemberNamingConvention
    {
        public StringComparer Comparer { get; }

        public NameConvention()
        {
            Comparer = StringComparer.Ordinal;
        }

        public string Convert(string name)
        {
            return name.ToLower();
        }
    }
}
