using ControllerLogger.Application.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControllerLogger.Application.Configuration
{
    public class SignalControllerDownloaderConfiguration
    {
        public SignalControllerType ControllerType { get; set; }
        public string LocalPath { get; set; }
        public int ConnectionTimeout { get; set; }
        public int ReadTimeout { get; set; } 
        public bool DeleteAfterDownload { get; set; }
        public DateTime EarliestAcceptableDate { get; set; }
        public bool RenameDuplicateFiles { get; set; }
    }
}
