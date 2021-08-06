using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class Faq
    {
        public int Faqid { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public int OrderNumber { get; set; }
    }
}
