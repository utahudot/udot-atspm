using ATSPM.Application.Common;
using System;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions for <see cref="double"/>
    /// </summary>
    public static class DoubleExtensions
    {
        /// <summary>
        /// Checks if <paramref name="value1"/> and <paramref name="value2"/> are equal
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AreEqual(this double value1, double value2, double tolerance = AtspmConstants.Tolerance)
        {
            return Math.Abs(value1 - value2) < tolerance;
        }

        /// <summary>
        /// Checks if <paramref name="value1"/> and <paramref name="value2"/> are not equal
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool AreNotEqual(this double value1, double value2, double tolerance = AtspmConstants.Tolerance)
        {
            return !value1.AreEqual(value2, tolerance);
        }

        /// <summary>
        /// Checks if <paramref name="value1"/> is greater than <paramref name="value2"/>
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsGreaterThan(this double value1, double value2, double tolerance = AtspmConstants.Tolerance)
        {
            return (value1 - value2) > tolerance;
        }

        /// <summary>
        /// Checks is <paramref name="value1"/> is less than <paramref name="value2"/>
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsLessThan(this double value1, double value2, double tolerance = AtspmConstants.Tolerance)
        {
            return (value2 - value1) > tolerance;
        }

        /// <summary>
        /// Checks if <paramref name="value1"/> is greater or equal to <paramref name="value2"/>
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsGreaterThanOrEqual(this double value1, double value2, double tolerance = AtspmConstants.Tolerance)
        {
            return value1.IsGreaterThan(value2, tolerance) || value1.AreEqual(value2, tolerance);
        }

        /// <summary>
        /// Checks if <paramref name="value1"/> is less than or equal to <paramref name="value2"/>
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsLessThanOrEqual(this double value1, double value2, double tolerance = AtspmConstants.Tolerance)
        {
            return value1.IsLessThan(value2, tolerance) || value1.AreEqual(value2, tolerance);
        }
    }
}
