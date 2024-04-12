using System;
using System.Collections.Generic;

namespace ATSPM.Domain.Common
{
    /// <summary>
    /// Versitile IEqualityComparer implementation that provides passing of delegate functions
    /// </summary>
    /// <typeparam name="T">Type to compare</typeparam>
    /// <example>
    /// <code>new LambdaEqualityComparer((x, y) => Equals(x, y))</code>
    /// </example>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1?view=net-5.0">Wiki</seealso>
    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _lambdaComparer;
        private readonly Func<T, int> _lambdaHash;

        /// <summary>
        /// Creates a new IEqualityComparer with delegate functions
        /// </summary>
        /// <param name="lambdaComparer">Function that defines what to compare</param>
        public LambdaEqualityComparer(Func<T, T, bool> lambdaComparer) : this(lambdaComparer, o => 0)
        {
        }

        /// <summary>
        /// Creates a new IEqualityComparer with delegate functions
        /// </summary>
        /// <param name="lambdaComparer">Function that defines what to compare</param>
        /// <param name="lambdaHash">Function that defines how to generate HashCode</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LambdaEqualityComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
        {
            _lambdaComparer = lambdaComparer ?? throw new ArgumentNullException(nameof(lambdaComparer));
            _lambdaHash = lambdaHash ?? throw new ArgumentNullException(nameof(lambdaHash));
        }

        /// <summary>
        /// Compares two objects of type "T" using supplied delegate functions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Returns true if equal</returns>
        public bool Equals(T x, T y)
        {
            return _lambdaComparer(x, y);
        }

        /// <summary>
        /// Generates hashcode based on supplied delegate function
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>HashCode</returns>
        public int GetHashCode(T obj)
        {
            return _lambdaHash(obj);
        }
    }

    /// <summary>
    /// Versitile IComparer implementation that provides passing of delegate functions
    /// </summary>
    /// <typeparam name="T">Type to compare</typeparam>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1?view=net-5.0">Wiki</seealso>
    public class LambdaComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _lambdaComparer;

        /// <summary>
        /// Creates a new IComparerwith delegate function
        /// </summary>
        /// <param name="lambdaComparer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public LambdaComparer(Func<T, T, int> lambdaComparer) => _lambdaComparer = lambdaComparer ?? throw new ArgumentNullException(nameof(lambdaComparer));

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other based on a delegate function
        /// </summary>
        /// <param name="x">First object to compare</param>
        /// <param name="y">Second object to compare</param>
        /// <returns></returns>
        public int Compare(T x, T y)
        {
            return _lambdaComparer(x, y);
        }
    }
}
