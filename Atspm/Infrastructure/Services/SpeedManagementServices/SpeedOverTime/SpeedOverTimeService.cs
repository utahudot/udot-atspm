using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedOverTime;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.SpeedOverTime
{
    public class SpeedOverTimeService : ReportServiceBase<SpeedOverTimeOptions, SpeedOverTimeDto>
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;
        private readonly IMonthlyAggregationRepository monthlyAggregationRepository;

        public SpeedOverTimeService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository, IMonthlyAggregationRepository monthlyAggregationRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
            this.monthlyAggregationRepository = monthlyAggregationRepository;
        }

        public override async Task<SpeedOverTimeDto> ExecuteAsync(SpeedOverTimeOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var segment = await segmentRepository.LookupAsync(parameter.SegmentId);
            List<SpeedDataDto> speedResult = new List<SpeedDataDto>();
            switch (parameter.TimeOptions)
            {
                case TimeOptionsEnum.Day:
                    var hourlyResult = await hourlySpeedRepository.GetHourlySpeedsForSegmentInSource(parameter, Guid.Parse(parameter.SegmentId));
                    speedResult = ConvertHourlyToSpeedData(hourlyResult);
                    break;
                case TimeOptionsEnum.Week:
                    var weeklyResult = await hourlySpeedRepository.GetWeeklySpeedsForSegmentInSource(parameter, Guid.Parse(parameter.SegmentId));
                    speedResult = ConvertWeeklyToSpeedData(weeklyResult, parameter.StartDate);
                    break;
                case TimeOptionsEnum.Month:
                    var monthlyResult = await monthlyAggregationRepository.SelectMonthlyAggregationBySegment(Guid.Parse(parameter.SegmentId));
                    speedResult = ConvertMonthlyToSpeedData(monthlyResult);
                    break;
                default:
                    break;
            }
            var result = new SpeedOverTimeDto()
            {
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                SpeedLimit = segment.SpeedLimit,
                StartingMilePoint = segment.StartMilePoint,
                EndingMilePoint = segment.EndMilePoint,
                TimeOptions = parameter.TimeOptions,
                Data = speedResult,
            };
            return result;
        }

        private static List<SpeedDataDto> ConvertMonthlyToSpeedData(List<MonthlyAggregation> monthlyResult)
        {
            var data = new List<SpeedDataDto>();
            var averageData = new List<DataPoint<double>>();
            var eightyFifthData = new List<DataPoint<long>>();

            foreach (var monthlyAggregation in monthlyResult)
            {
                averageData.Add(new DataPoint<double>(monthlyAggregation.BinStartTime, monthlyAggregation.AllDayAverageSpeed.Value));
            }

            var series = new AverageAndEightyFifthSeriesData()
            {
                Average = averageData,
                EightyFifth = eightyFifthData,
            };

            var dailyData = new SpeedDataDto()
            {
                Series = series,
            };

            data.Add(dailyData);

            return data;
        }

        private static List<SpeedDataDto> ConvertWeeklyToSpeedData(List<HourlySpeed> weeklyResult, DateOnly startDate)
        {
            var data = new List<SpeedDataDto>();
            var averageData = new List<DataPoint<double>>();
            var eightyFifthData = new List<DataPoint<long>>();

            if (weeklyResult[0].Date < startDate.ToDateTime(new TimeOnly(0, 0)))
            {
                weeklyResult[0].Date = startDate.ToDateTime(new TimeOnly(0, 0));
            }

            foreach (var week in weeklyResult)
            {
                averageData.Add(new DataPoint<double>(week.Date, week.Average));
                if (week.EightyFifthSpeed != null)
                {
                    eightyFifthData.Add(new DataPoint<long>(week.Date, (long)(week.EightyFifthSpeed ?? 0)));
                }
            }

            var series = new AverageAndEightyFifthSeriesData()
            {
                Average = averageData,
                EightyFifth = eightyFifthData,
            };

            var dailyData = new SpeedDataDto()
            {
                Series = series,
            };

            data.Add(dailyData);

            return data;
        }

        private static List<SpeedDataDto> ConvertHourlyToSpeedData(List<HourlySpeed> hourlyResult)
        {
            var grouping = hourlyResult.GroupBy(h => h.Date);
            var data = new List<SpeedDataDto>();

            foreach (var group in grouping)
            {
                var averageData = group.Select(h => new DataPoint<double>(h.BinStartTime, h.Average)).ToList();
                var eightyFifthData = group.Select(h => h.EightyFifthSpeed).All(speed => speed == null)
                    ? null
                    : group.Select(h => new DataPoint<long>(h.BinStartTime, (long)(h.EightyFifthSpeed ?? 0))).ToList();

                var series = new AverageAndEightyFifthSeriesData()
                {
                    Average = averageData,
                    EightyFifth = eightyFifthData,
                };

                var dailyData = new SpeedDataDto()
                {
                    Date = group.Key,
                    Series = series,
                };

                data.Add(dailyData);
            }

            return data;
        }
    }
}