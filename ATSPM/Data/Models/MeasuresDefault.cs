using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class MeasuresDefault
    {
        public string Measure { get; set; } = null!;
        public string OptionName { get; set; } = null!;
        public string? Value { get; set; }
    }
}
