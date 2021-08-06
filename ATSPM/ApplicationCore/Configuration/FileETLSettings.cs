using System;
using System.Collections.Generic;
using System.Text;

namespace ControllerLogger.Configuration
{
    public class FileETLSettings
    {
        public string RootPath { get; set; }
        public DateTime EarliestAcceptableDate { get; set; }
        public bool DeleteFile { get; set; }
        public int MaxThreadsMain { get; set; }
    }
}
