using Microsoft.EntityFrameworkCore;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    public static class ILocationRepositoryExtensions
    {
        public static IAsyncEnumerable<Location> GetEventsForAggregation(this ILocationRepository repo, DateTime aggregationDate, EventAggregationQueryOptions queryOptions)
        {
            var query = repo.GetList()
                .Include(i => i.Approaches).ThenInclude(i => i.Detectors).ThenInclude(i => i.DetectionTypes).ThenInclude(i => i.MeasureTypes)
                .Include(i => i.Approaches).ThenInclude(i => i.DirectionType)
                .Include(i => i.Areas)
                .Where(location => location.Start <= aggregationDate)
                .FromSpecification(new ActiveLocationSpecification());

            if (queryOptions.IncludedLocations?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedLocations.Contains(w.LocationIdentifier));
            }

            if (queryOptions.ExcludedLocations?.Any() == true)
            {
                query = query.Where(w => !queryOptions.ExcludedLocations.Contains(w.LocationIdentifier));
            }

            if (queryOptions.IncludedAreas?.Any() == true)
            {
                query = query.Where(w => w.Areas.Any(a => queryOptions.IncludedAreas.Contains(a.Name)));
            }

            if (queryOptions.IncludedRegions?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedRegions.Contains(w.Region.Description));
            }

            if (queryOptions.IncludedJurisdictions?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedJurisdictions.Contains(w.Jurisdiction.Name));
            }

            if (queryOptions.IncludedLocationTypes?.Any() == true)
            {
                query = query.Where(w => queryOptions.IncludedLocationTypes.Contains(w.LocationType.Name));
            }

            var result = query
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .AsQueryable()
                .AsAsyncEnumerable()
                .OrderBy(o => o.LocationIdentifier);

            return result;
        }
    }
}
