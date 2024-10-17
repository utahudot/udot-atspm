﻿using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Business.Watchdog
{
    public class WatchDogIssueTypeGroup
    {
        public WatchDogIssueTypes IssueType { get; set; }
        public List<WatchDogProductInfo> Products { get; set; }
        public string Name { get; set; }

        public WatchDogIssueTypeGroup()
        {
            Products = new List<WatchDogProductInfo>();
        }
    }
}
