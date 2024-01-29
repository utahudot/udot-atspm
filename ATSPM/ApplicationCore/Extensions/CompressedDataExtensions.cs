using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions to help with compressed repositories
    /// </summary>
    public static class CompressedDataExtensions
    {
        /// <summary>
        /// Takes the <see cref="ILocationLayer"/> and populates it based on the <see cref="CompressedDataBase.LocationIdentifier"/>
        /// Compressing the <see cref="ILocationLayer"/> to the data is inefficient so this is used to populate it upon decompression.
        /// </summary>
        /// <typeparam name="T">Object type of the compressed data that the <see cref="ILocationLayer"/> needs to be populated</typeparam>
        /// <param name="items"><see cref="CompressedDataBase"/> items that need the <see cref="ILocationLayer"/> populated</param>
        /// <returns></returns>
        public static IReadOnlyList<T> AddLocationIdentifer<T>(this IEnumerable<CompressedDataBase> items)
        {
            return items.SelectMany(m => (IEnumerable<T>)m.Data, (o, r) =>
            {
                if (r is ILocationLayer l)
                    l.LocationIdentifier = o.LocationIdentifier;
                return r;
            }).ToList();
        }
    }
}
