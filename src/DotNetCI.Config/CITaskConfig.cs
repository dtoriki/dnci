using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetCI.Config
{
    public class CITaskConfig
    {
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IEnumerable<string> Definitions { get; set; } = new List<string>();
        public IEnumerable<CIJobConfig> Jobs { get; set; } = new List<CIJobConfig>();
    }
}
