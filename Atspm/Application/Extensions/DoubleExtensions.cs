#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/DoubleExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Common;

namespace Utah.Udot.Atspm.Extensions
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
            return value1 - value2 > tolerance;
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
            return value2 - value1 > tolerance;
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
