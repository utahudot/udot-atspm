using ATSPM.Data.Interfaces;
using ATSPM.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    /// <summary>
    /// Extensions for object that inherit <see cref="ILocationApproachLayer"/>
    /// </summary>
    public static class LocationApproachLayerExtensions
    {
        /// <summary>
        /// Gets all items that have the provided <paramref name="approachId"/>
        /// </summary>
        /// <typeparam name="T"><see cref="ILocationApproachLayer"/></typeparam>
        /// <param name="items"><see cref="IEnumerable{T}"/> of objects that inherit <see cref="ILocationApproachLayer"/></param>
        /// <param name="approachId">Id of approach to match with <paramref name="items"/></param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetByApproachId<T>(this IEnumerable<T> items, int approachId) where T : ILocationApproachLayer
        {
            return items.Where(w => w.ApproachId == approachId).ToList();
        }

        /// <summary>
        /// Gets all items that have the provided <paramref name="approachId"/>
        /// </summary>
        /// <typeparam name="T"><see cref="ILocationApproachLayer"/></typeparam>
        /// <param name="items"><see cref="IEnumerable{T}"/> of objects that inherit <see cref="ILocationApproachLayer"/></param>
        /// <param name="approachId">Id of approach to match with <paramref name="items"/></param>
        /// <param name="protectedPhase">Flag to check protected phase status</param>
        /// <returns></returns>
        public static IReadOnlyList<T> GetByApproachId<T>(this IEnumerable<T> items, int approachId, bool protectedPhase) where T : ILocationApproachLayer
        {
            return items.Where(w => w.ApproachId == approachId && w.HasProperty("IsProtectedPhase") ? w.GetPropertyValue<bool>("IsProtectedPhase") == protectedPhase : true).ToList();
        }
    }
}
