using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class ImpactService
    {
        private readonly IImpactRepository impactRepository;
        private readonly ISegmentImpactRepository segmentImpactRepository;
        private readonly IImpactTypeRepository impactTypeRepository;
        private readonly IRouteRepository segmentRepository;

        public ImpactService(IImpactRepository impactRepository, ISegmentImpactRepository segmentImpactRepository, IImpactTypeRepository impactTypeRepository, IRouteRepository segmentRepository)
        {
            this.impactRepository = impactRepository;
            this.segmentImpactRepository = segmentImpactRepository;
            this.impactTypeRepository = impactTypeRepository;
            this.segmentRepository = segmentRepository;
        }

        public async Task<IReadOnlyList<Impact>> ListImpacts()
        {
            var impactsMissingFields = impactRepository.GetList();
            List<Impact> impacts = new List<Impact>();

            foreach (Impact impactMissingFields in impactsMissingFields)
            {
                Impact impact = await PopulateImpactAsync(impactMissingFields);
                impacts.Add(impact);
            }

            return impacts;
        }

        public async Task<Impact> GetImpactById(int id)
        {
            var impact = await impactRepository.LookupAsync(id);
            return await PopulateImpactAsync(impact);
        }

        public async Task<Impact> UpsertImpact(Impact impact)
        {
            //Add the impact
            var updatedImpact = await impactRepository.UpdateImpactAsync(impact);
            if (updatedImpact.Id == null)
            {
                return null;
            }
            var impactId = (int)updatedImpact.Id;
            //Add the impacted segments
            var segmentImpactList = impact.SegmentIds
            .Select(segmentId => new SegmentImpact
            {
                ImpactId = impactId,
                SegmentId = segmentId
            })
            .ToList();
            await segmentImpactRepository.UpdateRangeAsync(segmentImpactList);

            return await GetImpactById(impactId);
        }

        public async Task<Impact> UpsertImpactedSegment(int impactId, int segmentId)
        {
            await segmentImpactRepository.UpdateAsync(new SegmentImpact { ImpactId = impactId, SegmentId = segmentId });
            return await GetImpactById(impactId);
        }

        public async Task DeleteImpact(Impact existingImpact)
        {
            if (existingImpact.Id == null)
            {
                return;
            }
            await segmentImpactRepository.RemoveAllSegmentsFromImpactIdAsync(existingImpact.Id);
            await impactRepository.RemoveAsync(existingImpact);
        }

        public async Task<Impact> DeleteImpactedSegment(int impactId, int segmentId)
        {
            await segmentImpactRepository.RemoveAsync(new SegmentImpact { ImpactId = impactId, SegmentId = segmentId });
            return await GetImpactById(impactId);
        }

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private async Task<List<Route>> GetImpactTypesAsync(List<int> ids)
        {
            var tasks = ids.Select(id => segmentRepository.LookupAsync(id));
            Route[] routes = await Task.WhenAll(tasks);
            return routes.ToList();
        }

        private async Task<Impact> PopulateImpactAsync(Impact impact)
        {
            if (impact == null || impact.Id == null)
            {
                return null;
            }
            IReadOnlyList<SegmentImpact> segmentImpacts = await segmentImpactRepository.GetSegmentsForImpactAsync((int)impact.Id);
            List<int> segmentIds = segmentImpacts.Select(i => i.SegmentId).ToList();
            List<Route> segments = await GetImpactTypesAsync(segmentIds);
            ImpactType impactType = await impactTypeRepository.LookupAsync(impact.Id);
            Impact impactCopy = new Impact
            {
                Id = impact.Id,
                Description = impact.Description,
                Start = impact.Start,
                End = impact.End,
                StartMile = impact.StartMile,
                EndMile = impact.EndMile,
                Shape = impact.Shape,
                ImpactTypeId = impact.ImpactTypeId,
                ImpactType = impactType,
                CreatedOn = impact.CreatedOn,
                CreatedBy = impact.CreatedBy,
                UpdatedOn = impact.UpdatedOn,
                UpdatedBy = impact.UpdatedBy,
                DeletedOn = impact.DeletedOn,
                DeletedBy = impact.DeletedBy,
                SegmentIds = segmentIds,
                Segments = segments
            };
            return impactCopy;
        }

    }
}