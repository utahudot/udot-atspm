using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ExternalLink
    {
        public int ExternalLinkId { get; set; }
        public string Name { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }
}
