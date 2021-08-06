using System;
using System.Collections.Generic;
using System.Text;

namespace ControllerLogger.Helpers
{
    public static class PercentageHelpers
    {
        public static double ToPercent(this int a, int b)
        {
            return (a / b * 100);
        }

        public static double ToPercent(this double a, double b)
        {
            return (a / b * 100);
        }
    }
}
