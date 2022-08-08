using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Faq
    {
        public int Faqid { get; set; }
        public string Header { get; set; } = null!;
        public string Body { get; set; } = null!;
        public int OrderNumber { get; set; }
    }
}
