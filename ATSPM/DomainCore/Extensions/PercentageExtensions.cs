using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    public static class PercentageExtensions
    {
        public static double ToPercent(this int a, int b)
        {
            return a / b * 100;
        }

        public static double ToPercent(this double a, double b)
        {
            return a / b * 100;
        }
    }
}
