using ATSPM.Data.Models.AggregationModels;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections;

#nullable disable

namespace ATSPM.Data.Utility
{
    /// <summary>
    /// <see cref="ValueComparer"/> used to compare an <see cref="IEnumerable"/> of <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompressedListComparer<T> : ValueComparer<IEnumerable<T>>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public CompressedListComparer() : base(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList())
        { }
    }
}
