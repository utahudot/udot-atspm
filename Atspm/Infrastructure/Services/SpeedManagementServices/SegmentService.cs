using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices
{
    public class SegmentService
    {
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentEntityRepository _segmentEntityRepository;
        private readonly IHourlySpeedRepository _hourlySpeedRepository;

        public SegmentService(ISegmentRepository routeRepository, ISegmentEntityRepository segmentEntityRepository, IHourlySpeedRepository hourlySpeedRepository)
        {
            _segmentRepository = routeRepository;
            _segmentEntityRepository = segmentEntityRepository;
            _hourlySpeedRepository = hourlySpeedRepository;
        }

        // Add Segment
        public async Task AddSegment(Segment segment)
        {
            if (!IsValidSegment(segment))
            {
                throw new Exception("Invalid segment data. Make sure all required fields are provided and entities are not null.");
            }

            if (segment.Entities == null || !segment.Entities.Any())
            {
                throw new Exception("Segment must have at least one associated SegmentEntity.");
            }

            try
            {
                await _segmentRepository.AddAsync(segment);
                await _segmentEntityRepository.AddEntitiesAsync(segment.Entities);
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new Exception("Error occurred while adding the segment.", ex);
            }
        }

        // Update Segment
        public async Task UpdateSegment(Segment segment)
        {
            if (!IsValidSegment(segment))
            {
                throw new Exception("Invalid segment data. Make sure all required fields are provided and entities are not null.");
            }

            if (segment.Entities == null || !segment.Entities.Any())
            {
                throw new Exception("Segment must have at least one associated SegmentEntity.");
            }

            try
            {
                await _segmentRepository.UpdateAsync(segment);
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new Exception("Error occurred while updating the segment.", ex);
            }
        }

        // Delete Segment
        public async Task DeleteSegment(Guid segmentId)
        {
            var segment = await _segmentRepository.LookupAsync(segmentId);
            if (segment == null)
            {
                throw new Exception("Segment not found.");
            }
            try
            {
                await _segmentRepository.RemoveAsync(segment);
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new Exception("Error occurred while deleting the segment.", ex);
            }
        }

        // Get Segment by Id
        public async Task<Segment> GetSegmentById(Guid segmentId)
        {
            var segment = await _segmentRepository.LookupAsync(segmentId);
            if (segment == null)
            {
                throw new Exception("Segment not found.");
            }
            try
            {
                return segment;
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new Exception("Error occurred while retrieving the segment.", ex);
            }
        }

        // Validation method for segment
        private static bool IsValidSegment(Segment segment)
        {
            if (segment == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(segment.UdotRouteNumber) || segment.StartMilePoint < 0 || segment.EndMilePoint < 0)
            {
                return false;
            }

            if (segment.Entities == null || segment.Entities.Any(e => e == null))
            {
                return false;
            }

            return true;
        }
    }
}