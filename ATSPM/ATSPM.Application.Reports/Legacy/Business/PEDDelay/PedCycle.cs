﻿using System;

namespace Legacy.Common.Business.PEDDelay
{
    public class PedCycle
    {
        public PedCycle(DateTime beginWalk, DateTime callRegistered)
        {
            BeginWalk = beginWalk;
            CallRegistered = callRegistered;
        }

        public DateTime BeginWalk { get; }

        public DateTime CallRegistered { get; }

        public double Delay => Math.Abs((BeginWalk - CallRegistered).TotalSeconds);
    }
}