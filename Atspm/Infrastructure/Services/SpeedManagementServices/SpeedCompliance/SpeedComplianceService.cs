using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedCompliance;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverDistance;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedOverDistance;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedCompliance
{
    public class SpeedComplianceService : ReportServiceBase<SpeedComplianceOptions, List<SpeedComplianceDto>>
    {
        private readonly SpeedOverDistanceService speedOverDistanceService;
        private readonly ISegmentRepository segmentRepository;

        public SpeedComplianceService(SpeedOverDistanceService speedOverDistanceService, ISegmentRepository segmentRepository)
        {
            this.speedOverDistanceService = speedOverDistanceService;
            this.segmentRepository = segmentRepository;
        }

        public override async Task<List<SpeedComplianceDto>> ExecuteAsync(SpeedComplianceOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var speedOverDistanceOption = new SpeedOverDistanceOptions
            {
                StartDate = parameter.StartDate,
                EndDate = parameter.EndDate,
                SegmentIds = parameter.SegmentIds
            };
            var speedOverDistance = await speedOverDistanceService.ExecuteAsync(speedOverDistanceOption, progress, cancelToken);
            var result = new List<SpeedComplianceDto>();
            foreach (var speed in speedOverDistance)
            {
                var speedOverDistanceDto = new SpeedComplianceDto
                {
                    SegmentId = speed.SegmentId,
                    SegmentName = speed.SegmentName,
                    SpeedLimit = speed.SpeedLimit,
                    StartingMilePoint = speed.StartingMilePoint,
                    EndingMilePoint = speed.EndingMilePoint,
                    StartDate = speed.StartDate,
                    EndDate = speed.EndDate,
                    Average = speed.Average,
                    AvgVsBaseSpeed = speed.Average - speed.SpeedLimit,
                    EightyFifth = speed.EightyFifth,
                    EightyFifthPercentileVsBaseSpeed = speed.EightyFifth - speed.SpeedLimit
                };

                result.Add(speedOverDistanceDto);
            }
            return result.OrderBy(x => x.StartingMilePoint).ToList();
        }

    }
}