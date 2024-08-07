using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using System.Threading.Tasks.Dataflow;

namespace SpeedManagementApi.Processors
{
    public class AggregateMonthlyEventsProcessor
    {
        private readonly MonthlyAggregationService monthlyAggregationService;
        private readonly HourlySpeedService hourlySpeedService;
        private readonly ISegmentRepository segmentRepository;

        public AggregateMonthlyEventsProcessor(MonthlyAggregationService monthlyAggregationService, HourlySpeedService hourlySpeedService, ISegmentRepository segmentRepository)
        {
            this.monthlyAggregationService = monthlyAggregationService;
            this.hourlySpeedService = hourlySpeedService;
            this.segmentRepository = segmentRepository;
        }

        public async Task AggregateMonthlyEvents()
        {
            var settings = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 10,
            };
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            //List out the steps 
            var expiredEvents = new TransformManyBlock<object, MonthlyAggregationProcessorDto>(input => getAllSegments());
            //Set up monthlyAggregationProcessor DTO
            var allTimeData = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(async monthlyAggregationProcessor =>
            {
                var result = await PullOutAllMonthlyDataForSegment(monthlyAggregationProcessor);
                return result;
            }, settings);
            var allDay = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForAllDay, settings);
            var offPeak = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForOffPeak, settings);
            var amPeak = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForAmPeak, settings);
            var pmPeak = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForPmPeak, settings);
            var midDay = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForMidDay, settings);
            var evening = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForEvening, settings);
            var earlyMorning = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForEarlyMorning, settings);


            //Save the information
            var saveBlock = new ActionBlock<MonthlyAggregationProcessorDto>(UpsertMonthlyAggregation, settings);

            //This is very important for batching
            DataflowLinkOptions linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            //Link to the workflow
            expiredEvents.LinkTo(allTimeData, linkOptions);
            allTimeData.LinkTo(allDay, linkOptions);
            allDay.LinkTo(offPeak, linkOptions);
            offPeak.LinkTo(amPeak, linkOptions);
            amPeak.LinkTo(pmPeak, linkOptions);
            pmPeak.LinkTo(midDay, linkOptions);
            midDay.LinkTo(evening, linkOptions);
            evening.LinkTo(earlyMorning, linkOptions);
            earlyMorning.LinkTo(saveBlock, linkOptions);

            //Start the workflow
            await expiredEvents.SendAsync("");
            expiredEvents.Complete();

            await saveBlock.Completion;

            // _timer = new Timer(StartWorkflow, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async Task AggregateMonthlyEventsForSingleSegment(MonthlyAggregation monthlyAggregation)
        {
            var settings = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 10,
            };
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            //List out the steps 
            var expiredEvents = new TransformManyBlock<MonthlyAggregation, MonthlyAggregationProcessorDto>(GenerateTwoYearsForASegment);
            //Set up monthlyAggregationProcessor DTO
            var allTimeData = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(PullOutAllMonthlyDataForSegment, settings);
            var allDay = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForAllDay, settings);
            var offPeak = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForOffPeak, settings);
            var amPeak = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForAmPeak, settings);
            var pmPeak = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForPmPeak, settings);
            var midDay = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForMidDay, settings);
            var evening = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForEvening, settings);
            var earlyMorning = new TransformBlock<MonthlyAggregationProcessorDto, MonthlyAggregationProcessorDto>(GetHourlySpeedsForEarlyMorning, settings);


            //Save the information
            var saveBlock = new ActionBlock<MonthlyAggregationProcessorDto>(UpsertMonthlyAggregation, settings);

            //This is very important for batching
            DataflowLinkOptions linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            //Link to the workflow
            expiredEvents.LinkTo(allTimeData, linkOptions);
            allTimeData.LinkTo(allDay, linkOptions);
            allDay.LinkTo(offPeak, linkOptions);
            offPeak.LinkTo(amPeak, linkOptions);
            amPeak.LinkTo(pmPeak, linkOptions);
            pmPeak.LinkTo(midDay, linkOptions);
            midDay.LinkTo(evening, linkOptions);
            evening.LinkTo(earlyMorning, linkOptions);
            earlyMorning.LinkTo(saveBlock, linkOptions);

            //Start the workflow
            await expiredEvents.SendAsync(monthlyAggregation);
            expiredEvents.Complete();

            await saveBlock.Completion;

            // _timer = new Timer(StartWorkflow, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        }

        ///////////////////// 
        //PRIVATE FUNCTIONS//
        /////////////////////
        private IEnumerable<MonthlyAggregationProcessorDto> getAllSegments()
        {
            DateTime today = DateTime.Today;
            DateTime lastDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddDays(-1);
            DateTime firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var allSegments = segmentRepository.AllSegmentsWithEntity();
            foreach (var segment in allSegments)
            {
                foreach (var segmentEntity in segment.RouteEntities)
                {
                    yield return new MonthlyAggregationProcessorDto
                    {
                        hourlySpeeds = new List<HourlySpeed>(),
                        startDate = firstDayOfPreviousMonth,
                        endDate = lastDayOfPreviousMonth,
                        SegmentId = segment.Id,
                        monthlyAggregation = new MonthlyAggregation
                        {
                            BinStartTime = firstDayOfPreviousMonth,
                            SegmentId = segment.Id,
                            SourceId = segmentEntity.SourceId,
                            DataQuality = true
                        }
                    };
                }
            }
        }

        private IEnumerable<MonthlyAggregationProcessorDto> GenerateTwoYearsForASegment(MonthlyAggregation monthlyAggregation)
        {
            DateTime today = DateTime.Today;
            DateTime lastDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddDays(-1);
            DateTime firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            DateTime endCondition = new DateTime(today.Year, today.Month, 1).AddMonths(-1).AddYears(-2);
            var allSegments = segmentRepository.AllSegmentsWithEntity();

            while (firstDayOfPreviousMonth >= endCondition)
            {
                yield return new MonthlyAggregationProcessorDto
                {
                    hourlySpeeds = new List<HourlySpeed>(),
                    startDate = firstDayOfPreviousMonth,
                    endDate = lastDayOfPreviousMonth,
                    SegmentId = monthlyAggregation.SegmentId,
                    monthlyAggregation = new MonthlyAggregation
                    {
                        BinStartTime = firstDayOfPreviousMonth,
                        SegmentId = monthlyAggregation.SegmentId,
                        SourceId = monthlyAggregation.SourceId,
                        DataQuality = true
                    }
                };
                lastDayOfPreviousMonth = firstDayOfPreviousMonth.AddDays(-1); ;
                firstDayOfPreviousMonth = firstDayOfPreviousMonth.AddMonths(-1);
            }
        }
        private async Task UpsertMonthlyAggregation(MonthlyAggregationProcessorDto monthlyAggregationProcess)
        {
            await monthlyAggregationService.UpsertMonthlyAggregation(monthlyAggregationProcess.monthlyAggregation);
        }

        private async Task<MonthlyAggregationProcessorDto> PullOutAllMonthlyDataForSegment(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            MonthlyAggregation monthlyAggregation = monthlyAggregationProcessor.monthlyAggregation;
            DateTime newStartDate = monthlyAggregationProcessor.startDate.AddDays(-1);
            DateTime newEndDate = monthlyAggregationProcessor.endDate.AddDays(1);
            DateTime startTime = new DateTime(1, 1, 1, 0, 0, 0); // 12:00 AM
            DateTime endTime = new DateTime(1, 1, 1, 23, 59, 59); // 11:59 PM

            Guid segmentId = monthlyAggregation.SegmentId;
            monthlyAggregationProcessor.hourlySpeeds = await hourlySpeedService.GetHourlySpeedsForTimePeriod(segmentId, newStartDate, newEndDate, startTime, endTime);
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForAllDay(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(0, 0, 0); // 12:00 AM
            TimeSpan endTime = new TimeSpan(23, 59, 59); // 11:59 PM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForOffPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(22, 0, 0); // 10:00 PM
            TimeSpan endTime = new TimeSpan(4, 0, 0); // 4:00 AM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriodWithOvernightMetric(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForAmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(6, 0, 0); // 6:00 AM
            TimeSpan endTime = new TimeSpan(9, 0, 0);   // 9:00 AM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForPmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(16, 0, 0); // 4:00 PM
            TimeSpan endTime = new TimeSpan(18, 0, 0);   // 6:00 PM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForMidDay(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            TimeSpan endTime = new TimeSpan(16, 0, 0);   // 4:00 PM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEvening(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(18, 0, 0); // 6:00 PM
            TimeSpan endTime = new TimeSpan(22, 0, 0);   // 10:00 PM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.EveningAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEarlyMorning(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(4, 0, 0); // 4:00 AM
            TimeSpan endTime = new TimeSpan(6, 0, 0);   // 6:00 AM

            var (averageSpeed, totalViolations, totalExtremeViolations) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningExtremeViolations = totalExtremeViolations;
            return monthlyAggregationProcessor;
        }

        private (long averageSpeed, long totalViolations, long totalExtremeViolations) GetAveragesOfTimePeriodWithOvernightMetric(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime)
        {
            TimeSpan midnight = new TimeSpan(0, 0, 0); // 12:00 AM
            TimeSpan almostMidnight = new TimeSpan(23, 59, 59); // 11:59 PM

            var filteredSpeeds = hourlySpeeds
                .Where(hs => hs.BinStartTime.TimeOfDay >= startTime && hs.BinStartTime.TimeOfDay <= almostMidnight)
                .ToList();

            var filteredSpeedsPost = hourlySpeeds
                .Where(hs => hs.BinStartTime.TimeOfDay >= midnight && hs.BinStartTime.TimeOfDay <= endTime)
                .ToList();

            filteredSpeeds.AddRange(filteredSpeedsPost);
            var averageSpeed = GetWeigthtedAverageSpeed(hourlySpeeds);
            var totalViolations = hourlySpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            var totalExtremeViolations = hourlySpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            return (averageSpeed, totalViolations, totalExtremeViolations);
        }

        private (long averageSpeed, long totalViolations, long totalExtremeViolations) GetAveragesOfTimePeriod(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime)
        {
            if (hourlySpeeds.Count == 0)
            {
                return (1, 1, 1);
            }
            var filteredByTime = hourlySpeeds
                .Where(hs => hs.BinStartTime.TimeOfDay >= startTime && hs.BinStartTime.TimeOfDay <= endTime)
                .ToList();
            if (filteredByTime.Count == 0)
            {
                return (1, 1, 1);
            }

            // Find the minimum and maximum dates in the filtered list
            var minDate = filteredByTime.Min(hs => hs.Date);
            var maxDate = filteredByTime.Max(hs => hs.Date);

            // Filter out the first and last dates
            var filteredSpeeds = filteredByTime
                .Where(hs => hs.Date != minDate && hs.Date != maxDate)
                .ToList();

            var averageSpeed = GetWeigthtedAverageSpeed(hourlySpeeds);
            var totalViolations = hourlySpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            var totalExtremeViolations = hourlySpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            return (averageSpeed, totalViolations, totalExtremeViolations);
        }

        private long GetWeigthtedAverageSpeed(List<HourlySpeed> hourlySpeeds)
        {
            var flow = hourlySpeeds.Sum(hs => hs.Flow ?? 1);
            var totalAverage = hourlySpeeds.Sum(hs => hs.Average);
            try
            {
                return (flow * totalAverage) / flow;
            }
            catch { return 0; }
        }

    }
}
