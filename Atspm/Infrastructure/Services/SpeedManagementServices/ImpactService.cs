using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class ImpactService
    {
        private readonly IImpactRepository impactRepository;
        private readonly ISegmentImpactRepository segmentImpactRepository;
        private readonly IImpactImpactTypeRepository impactImpactTypeRepository;
        private readonly IImpactTypeRepository impactTypeRepository;
        private readonly ISegmentRepository segmentRepository;

        public ImpactService(IImpactRepository impactRepository, ISegmentImpactRepository segmentImpactRepository, IImpactTypeRepository impactTypeRepository, ISegmentRepository segmentRepository, IImpactImpactTypeRepository impactImpactTypeRepository)
        {
            this.impactRepository = impactRepository;
            this.segmentImpactRepository = segmentImpactRepository;
            this.impactTypeRepository = impactTypeRepository;
            this.segmentRepository = segmentRepository;
            this.impactImpactTypeRepository = impactImpactTypeRepository;
        }

        public async Task<IReadOnlyList<Impact>> ListImpacts()
        {
            List<Impact> impacts = impactRepository.GetList().ToList();
            return impacts;
        }

        public async Task<Impact> GetImpactById(Guid id)
        {
            var impact = await impactRepository.LookupAsync(id);
            return await PopulateImpactAsync(impact);
        }

        public async Task<List<Impact>> GetImpactsOnSegment(Guid segmentId)
        {
            var impacts = await impactRepository.GetImpactsForSegmentAsync(segmentId);

            return impacts;
        }

        public async Task<Impact> UpsertImpact(Impact impact)
        {
            //Add the impact
            var updatedImpact = await impactRepository.UpdateImpactAsync(impact);
            if (updatedImpact.Id == null)
            {
                return null;
            }
            var impactId = (Guid)updatedImpact.Id;
            //Add the impacted segments
            var segmentImpactList = impact.SegmentIds != null
                 ? impact.SegmentIds
                     .Select(segmentId => new SegmentImpact
                     {
                         ImpactId = impactId,
                         SegmentId = segmentId
                     })
                     .ToList()
                 : new List<SegmentImpact>();

            var impactTypeList = impact.ImpactTypeIds != null ?
                impact.ImpactTypeIds
                    .Select(impactTypeId => new ImpactImpactType
                    {
                        ImpactId = impactId,
                        ImpactTypeId = impactTypeId
                    })
                    .ToList() : new List<ImpactImpactType>();

            await segmentImpactRepository.UpdateRangeAsync(segmentImpactList);
            await impactImpactTypeRepository.UpdateRangeAsync(impactTypeList);
            Impact currentImpact = await GetImpactById(impactId);

            return currentImpact;
        }

        public async Task<Impact> UpsertImpactedSegment(Guid impactId, Guid segmentId)
        {
            await segmentImpactRepository.UpdateAsync(new SegmentImpact { ImpactId = impactId, SegmentId = segmentId });
            return await GetImpactById(impactId);
        }

        public async Task<Impact> UpsertImpactedImpactType(Guid impactId, Guid impactTypeId)
        {
            await impactImpactTypeRepository.UpdateAsync(new ImpactImpactType { ImpactId = impactId, ImpactTypeId = impactTypeId });
            return await GetImpactById(impactId);
        }


        public async Task DeleteImpact(Impact existingImpact)
        {
            if (existingImpact.Id == null)
            {
                return;
            }
            await segmentImpactRepository.RemoveAllSegmentsFromImpactIdAsync(existingImpact.Id);
            await impactImpactTypeRepository.RemoveAllImpactTypesFromImpactIdAsync(existingImpact.Id);
            await impactRepository.RemoveAsync(existingImpact);
        }

        public async Task<Impact> DeleteImpactedSegment(Guid impactId, Guid segmentId)
        {
            await segmentImpactRepository.RemoveAsync(new SegmentImpact { ImpactId = impactId, SegmentId = segmentId });
            return await GetImpactById(impactId);
        }

        public async Task<Impact> DeleteImpactedImpactType(Guid impactId, Guid impactTypeId)
        {
            await impactImpactTypeRepository.RemoveAsync(new ImpactImpactType { ImpactId = impactId, ImpactTypeId = impactTypeId });
            return await GetImpactById(impactId);
        }

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////

        private async Task<List<Segment>> GetImpactTypesAsync(List<Guid> ids)
        {
            var tasks = ids.Select(id => segmentRepository.LookupAsync(id));
            Segment[] routes = await Task.WhenAll(tasks);
            return routes.ToList();
        }

        private async Task<Impact> PopulateImpactAsync(Impact impact)
        {
            if (impact == null || impact.Id == null)
            {
                return null;
            }
            var impactOutput = await impactRepository.GetInstanceDetails(impact.Id);

            return impactOutput;
        }

    }
}