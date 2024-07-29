using ATSPM.Application.Common;
using System;

namespace ATSPM.Application.Extensions
{
    public static class DoubleExtensions
    {
        public static bool AreEqual(this double value1, double value2, double tolerance = Constants.Tolerance)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }

        public static bool AreNotEqual(this double value1, double value2, double tolerance = Constants.Tolerance)
        {
            return !value1.AreEqual(value2, tolerance);
        }

        public static bool IsGreaterThan(this double value1, double value2, double tolerance = Constants.Tolerance)
        {
            return (value1 - value2) > tolerance;
        }

        public static bool IsLessThan(this double value1, double value2, double tolerance = Constants.Tolerance)
        {
            return (value2 - value1) > tolerance;
        }

        public static bool IsGreaterThanOrEqual(this double value1, double value2, double tolerance = Constants.Tolerance)
        {
            return value1.IsGreaterThan(value2, tolerance) || value1.AreEqual(value2, tolerance);
        }

        public static bool IsLessThanOrEqual(this double value1, double value2, double tolerance = Constants.Tolerance)
        {
            return value1.IsLessThan(value2, tolerance) || value1.AreEqual(value2, tolerance);
        }
    }
}
