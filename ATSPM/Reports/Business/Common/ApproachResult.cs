﻿using System;

namespace Reports.Business.Common
{
    public class ApproachResult : SignalResult
    {
        public int ApproachId { get; set; }

        public ApproachResult(int approachId, string signalId, DateTime start, DateTime end):base(signalId, start, end)
        {
            ApproachId = approachId;
        }
    }
}