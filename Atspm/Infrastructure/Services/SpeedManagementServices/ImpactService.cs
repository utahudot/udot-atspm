using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.SpeedManagementServices
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
            var impactsMissingFields = impactRepository.GetList();
            List<Impact> impacts = new List<Impact>();

            foreach (Impact impactMissingFields in impactsMissingFields)
            {
                Impact impact = await PopulateImpactAsync(impactMissingFields);
                impacts.Add(impact);
            }

            return impacts;
        }

        public async Task<Impact> GetImpactById(Guid id)
        {
            var impact = await impactRepository.LookupAsync(id);
            return await PopulateImpactAsync(impact);
        }

        public async Task<List<Impact>> GetImpactsOnSegment(Guid segmentId)
        {
            var segmentImpacts = await segmentImpactRepository.GetImpactsForSegmentAsync(segmentId);
            var impactIds = segmentImpacts.Select(it => it.ImpactId).ToList();
            var impactsMissingFields = await impactRepository.GetInstancesDetails(impactIds);
            List<Impact> impacts = new List<Impact>();

            foreach (Impact impactMissingFields in impactsMissingFields)
            {
                Impact impact = await PopulateImpactAsync(impactMissingFields);
                impacts.Add(impact);
            }
            return impacts;
        }

        public async Task<List<Impact>> GetListOfImpactsFromIds(List<Guid> impactIds)
        {
            var impactsMissingFields = await impactRepository.GetInstancesDetails(impactIds);
            List<Impact> impacts = new List<Impact>();

            foreach (Impact impactMissingFields in impactsMissingFields)
            {
                Impact impact = await PopulateImpactAsync(impactMissingFields);
                impacts.Add(impact);
            }
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

        private async Task<List<Data.Models.SpeedManagementConfigModels.Segment>> GetImpactTypesAsync(List<Guid> ids)
        {
            var tasks = ids.Select(id => segmentRepository.LookupAsync(id));
            Data.Models.SpeedManagementConfigModels.Segment[] routes = await Task.WhenAll(tasks);
            return routes.ToList();
        }

        private async Task<Impact> PopulateImpactAsync(Impact impact)
        {
            if (impact == null || impact.Id == null)
            {
                return null;
            }
            IReadOnlyList<SegmentImpact> segmentImpacts = await segmentImpactRepository.GetSegmentsForImpactAsync((Guid)impact.Id);
            List<Guid> segmentIds = segmentImpacts.Select(i => i.SegmentId).ToList();
            //List<Data.Models.SpeedManagementConfigModels.Segment> segments = await GetImpactTypesAsync(segmentIds);
            IReadOnlyList<ImpactImpactType> impactImpactTypes = await impactImpactTypeRepository.GetImpactTypesForImpactAsync((Guid)impact.Id);
            List<Guid> impactTypeIds = impactImpactTypes.Select(i => i.ImpactTypeId).ToList();

            var impactTypes = await Task.WhenAll(impactTypeIds.Select(async id => await impactTypeRepository.LookupAsync(id)));
            List<ImpactType> impactTypesList = impactTypes.ToList();

            Impact impactCopy = new Impact
            {
                Id = impact.Id,
                Description = impact.Description,
                Start = impact.Start,
                End = impact.End,
                StartMile = impact.StartMile,
                EndMile = impact.EndMile,
                ImpactTypeIds = impactTypeIds,
                ImpactTypes = impactTypesList,
                CreatedOn = impact.CreatedOn,
                CreatedBy = impact.CreatedBy,
                UpdatedOn = impact.UpdatedOn,
                UpdatedBy = impact.UpdatedBy,
                DeletedOn = impact.DeletedOn,
                DeletedBy = impact.DeletedBy,
                SegmentIds = segmentIds
            };
            return impactCopy;
        }

    }
}