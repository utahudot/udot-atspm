using ATSPM.Application.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Application.Configuration
{
    public class LocationControllerDownloaderConfiguration
    {
        public string LocalPath { get; set; }
        
        public int ConnectionTimeout { get; set; }
        
        public int ReadTimeout { get; set; }
        
        public bool DeleteFile { get; set; }
        
        public bool PingControllerToVerify { get; set; }
    }
}
