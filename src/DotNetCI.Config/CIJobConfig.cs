using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCI.Config
{
    public class CIJobConfig
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IEnumerable<string> Filters { get; set; } = new List<string>();
        [Required]
        public string Configuration { get; set; }
        public IEnumerable<string> Depends { get; set; } = new List<string>();
    }
}
