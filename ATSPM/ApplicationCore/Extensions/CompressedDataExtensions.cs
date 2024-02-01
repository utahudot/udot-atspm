using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions to help with compressed repositories
    /// </summary>
    public static class CompressedDataExtensions
    {
        public static void Test<T>(this IEventLogRepository<T> repo) where T : EventLogModelBase
        {
            
        }

        /// <summary>
        /// Get event logs from <see cref="CompressedEventLogs{T}"/>
        /// </summary>
        /// <typeparam name="T">Type from <see cref="EventLogModelBase"/></typeparam>
        /// <param name="items">List of <see cref="CompressedEventLogs{T}"/> to get events from</param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetEventLogs<T>(this IEnumerable<CompressedEventLogs<T>> items) where T : EventLogModelBase
        {
            return items.SelectMany(m => m.Data).ToList();
        }

        public static IReadOnlyList<T> GetEventLogs<T>(this CompressedEventLogs<T> item) where T : EventLogModelBase
        {
            return item.Data.ToList();
        }

        /// <summary>
        /// Takes the <see cref="ILocationLayer"/> and populates it based on the <see cref="CompressedDataBase.LocationIdentifier"/>
        /// Compressing the <see cref="ILocationLayer"/> to the data is inefficient so this is used to populate it upon decompression.
        /// </summary>
        /// <typeparam name="T">Object type of the compressed data that the <see cref="ILocationLayer"/> needs to be populated</typeparam>
        /// <param name="items"><see cref="CompressedDataBase"/> items that need the <see cref="ILocationLayer"/> populated</param>
        /// <returns></returns>
        public static IEnumerable<T> AddLocationIdentifer<T>(this IEnumerable<CompressedDataBase> items)
        {
            return items.SelectMany(m => (IEnumerable<T>)m.Data, (o, r) =>
            {
                if (r is ILocationLayer l)
                    l.LocationIdentifier = o.LocationIdentifier;
                return r;
            });
        }

        public static IEnumerable<T> AddLocationIdentifer<T>(this CompressedDataBase item)
        {
            if (item is ILocationLayer l)
            {
                foreach (var i in (IEnumerable<T>)item.Data)
                {
                    if (i is ILocationLayer d)
                    {
                        d.LocationIdentifier = l.LocationIdentifier;
                    }
                }
            }

            return (IEnumerable<T>)item.Data;
        }
    }
}
