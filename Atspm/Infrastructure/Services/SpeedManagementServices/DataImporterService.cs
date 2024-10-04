using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class DataImporterService
    {
        private readonly ISegmentRepository _segmentRepository;

        public DataImporterService(ISegmentRepository segmentRepository)
        {
            _segmentRepository = segmentRepository;
        }

        public async Task DownloadDataForEntities(List<SegmentEntity> entities)
        {
            var entitiesBySource = entities.GroupBy(entity => entity.SourceId);

            foreach (var sourceId in entitiesBySource)
            {
                switch (sourceId.Key)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public async Task DownloadDataForSegmentWithSource(Guid segmentId, List<long> sourceIds)
        {
            var segment = await _segmentRepository.LookupAsync(segmentId);
            if (segment == null)
            {
                throw new Exception("Segment not found.");
            }

            foreach (var sourceId in sourceIds) {
                switch (sourceId)
                {
                    case 1:
                        break;
                    case 2:

                        break;
                    case 3:
                        break;
                    default:
                        throw new Exception();
                }
            }

        }

        public async Task DownloadDataForSegment(Guid segment)
        {

        }
    }
}
