using System;
using System.Collections.Generic;
using System.Text;

namespace ControllerLogger.Configuration
{
    public class ControllerFTPSettings
    {
        public string RootPath { get; set; }
        public int PollTime { get; set; }
        public int TimeOffset { get; set; }
        public bool DeleteFile { get; set; }
        public int MaxThreadsMain { get; set; }
    }
}
