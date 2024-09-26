using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.MonthlyAggregation;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

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
            var batchSize = 500;
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
            var batchBlock = new BatchBlock<MonthlyAggregationProcessorDto>(batchSize);
            var saveBlock = new ActionBlock<IEnumerable<MonthlyAggregationProcessorDto>>(UpsertMonthlyAggregations, settings);

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
            earlyMorning.LinkTo(batchBlock, linkOptions);
            batchBlock.LinkTo(saveBlock, linkOptions);

            //Start the workflow
            await expiredEvents.SendAsync("");
            expiredEvents.Complete();

            await saveBlock.Completion;

            // _timer = new Timer(StartWorkflow, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async Task AggregateCertainMonthforSource(DateTime date, int source)
        {
            var batchSize = 500;
            var settings = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 10,
            };
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            //List out the steps 
            var expiredEvents = new TransformManyBlock<Tuple<DateTime, int>, MonthlyAggregationProcessorDto>(input => AllSegmentsFromSourceTimePeriod(input.Item1, input.Item2));
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
            var batchBlock = new BatchBlock<MonthlyAggregationProcessorDto>(batchSize);
            var saveBlock = new ActionBlock<IEnumerable<MonthlyAggregationProcessorDto>>(UpsertMonthlyAggregations, settings);

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
            earlyMorning.LinkTo(batchBlock, linkOptions);
            batchBlock.LinkTo(saveBlock, linkOptions);

            var tuple = new Tuple<DateTime, int>(date, source);
            //Start the workflow
            await expiredEvents.SendAsync(tuple);
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
                yield return new MonthlyAggregationProcessorDto
                {
                    hourlySpeeds = new List<HourlySpeed>(),
                    startDate = firstDayOfPreviousMonth,
                    endDate = lastDayOfPreviousMonth,
                    SegmentId = segment.Id,
                    monthlyAggregation = new MonthlyAggregation
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.SpecifyKind(today, DateTimeKind.Utc),
                        BinStartTime = firstDayOfPreviousMonth,
                        SegmentId = segment.Id,
                        //INSTEAD OF GET ALL SEGMENTS, DO GET SEGMENTS BY SOURCE ID, FOR KENZIE PROBABLY
                        SourceId = segment.Entities.FirstOrDefault().SourceId,
                        PercentObserved = 0.00
                    }
                };
            }
        }

        private IEnumerable<MonthlyAggregationProcessorDto> AllSegmentsFromSourceTimePeriod(DateTime date, int source)
        {
            DateTime today = date;
            DateTime lastDayOfMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var allSegments = segmentRepository.AllSegmentsWithEntity(source);
            foreach (var segment in allSegments)
            {
                yield return new MonthlyAggregationProcessorDto
                {
                    hourlySpeeds = new List<HourlySpeed>(),
                    startDate = firstDayOfMonth,
                    endDate = lastDayOfMonth,
                    SegmentId = segment.Id,
                    monthlyAggregation = new MonthlyAggregation
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.SpecifyKind(today, DateTimeKind.Utc),
                        BinStartTime = firstDayOfMonth,
                        SegmentId = segment.Id,
                        SourceId = segment.Entities.FirstOrDefault().SourceId,
                        PercentObserved = 0.00
                    }
                };
            }
        }

        private IEnumerable<MonthlyAggregationProcessorDto> GenerateTwoYearsForASegment(MonthlyAggregation monthlyAggregation)
        {
            DateTime today = DateTime.Today;
            DateTime lastDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddDays(-1);
            DateTime firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            DateTime endCondition = new DateTime(today.Year, today.Month, 1).AddMonths(-1).AddYears(-2);

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
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.SpecifyKind(today, DateTimeKind.Utc),
                        BinStartTime = firstDayOfPreviousMonth,
                        SegmentId = monthlyAggregation.SegmentId,
                        SourceId = monthlyAggregation.SourceId,
                        PercentObserved = 0.00
                    }
                };
                lastDayOfPreviousMonth = firstDayOfPreviousMonth.AddDays(-1); ;
                firstDayOfPreviousMonth = firstDayOfPreviousMonth.AddMonths(-1);
            }
        }


        private async void UpsertMonthlyAggregations(IEnumerable<MonthlyAggregationProcessorDto> enumerable)
        {
            var monthlyAggregations = enumerable.Select(x => x.monthlyAggregation);
            await monthlyAggregationService.UpsertMonthlyAggregations(monthlyAggregations);
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

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.AllDayViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayFlow = flow;
            monthlyAggregationProcessor.monthlyAggregation.PercentObserved = monthlyAggregationProcessor.hourlySpeeds.Average(hs => hs.PercentObserved);
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForOffPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(22, 0, 0); // 10:00 PM
            TimeSpan endTime = new TimeSpan(4, 0, 0); // 4:00 AM

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriodWithOvernightMetric(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakFlow = flow;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForAmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(6, 0, 0); // 6:00 AM
            TimeSpan endTime = new TimeSpan(9, 0, 0);   // 9:00 AM

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakFlow = flow;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForPmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(16, 0, 0); // 4:00 PM
            TimeSpan endTime = new TimeSpan(18, 0, 0);   // 6:00 PM

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakFlow = flow;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForMidDay(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            TimeSpan endTime = new TimeSpan(16, 0, 0);   // 4:00 PM

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.MidDayViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayFlow = flow;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEvening(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(18, 0, 0); // 6:00 PM
            TimeSpan endTime = new TimeSpan(22, 0, 0);   // 10:00 PM

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.EveningAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.EveningViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningFlow = flow;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEarlyMorning(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(4, 0, 0); // 4:00 AM
            TimeSpan endTime = new TimeSpan(6, 0, 0);   // 6:00 AM

            var (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthAverage, flow) = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime);

            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageSpeed = averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageEightyFifthSpeed = eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningViolations = totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningExtremeViolations = totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningFlow = flow;
            return monthlyAggregationProcessor;
        }

        private (double averageSpeed, long totalViolations, long totalExtremeViolations, double EightyFifthAverage, long flow) GetAveragesOfTimePeriodWithOvernightMetric(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime)
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
            var flow = filteredSpeeds.Sum(hs => hs.Flow ?? 0);
            var averageSpeed = GetWeigthtedAverageSpeed(filteredSpeeds);
            var eightyFifthSpeed = GetWeigthtedEightyFifthAverageSpeed(filteredSpeeds);
            var totalViolations = filteredSpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            var totalExtremeViolations = filteredSpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            return (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthSpeed, flow);
        }

        private (double averageSpeed, long totalViolations, long totalExtremeViolations, double EightyFifthAverage, long flow) GetAveragesOfTimePeriod(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime)
        {
            if (hourlySpeeds.Count == 0)
            {
                return (0, 0, 0, 0, 0);
            }
            var filteredByTime = hourlySpeeds
                .Where(hs => hs.BinStartTime.TimeOfDay >= startTime && hs.BinStartTime.TimeOfDay <= endTime)
                .ToList();
            if (filteredByTime.Count == 0)
            {
                return (0, 0, 0, 0, 0);
            }

            // Find the minimum and maximum dates in the filtered list
            var minDate = filteredByTime.Min(hs => hs.Date);
            var maxDate = filteredByTime.Max(hs => hs.Date);

            // Filter out the first and last dates
            var filteredSpeeds = filteredByTime
                .Where(hs => hs.Date != minDate && hs.Date != maxDate)
                .ToList();

            var flow = filteredSpeeds.Sum(hs => hs.Flow ?? 0);
            var averageSpeed = GetWeigthtedAverageSpeed(filteredSpeeds);
            var eightyFifthSpeed = GetWeigthtedEightyFifthAverageSpeed(filteredSpeeds);
            var totalViolations = filteredSpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            var totalExtremeViolations = filteredSpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            return (averageSpeed, totalViolations, totalExtremeViolations, eightyFifthSpeed, flow);
        }

        private int GetWeigthtedAverageSpeed(List<HourlySpeed> hourlySpeeds)
        {
            if (hourlySpeeds == null || hourlySpeeds.Count() == 0)
            {
                return 0;
            }
            double sumFlow = hourlySpeeds.Sum(hs => hs.Flow ?? 0);
            //if flow is all zero do normal average
            if (sumFlow <= 0)
            {
                double totalAverage = hourlySpeeds.Sum(hs => hs.Average);
                return (int)Math.Round(totalAverage / hourlySpeeds.Count());
            }
            var flowSpeed = new List<double>();
            foreach (var hourlySpeed in hourlySpeeds)
            {
                double flow = hourlySpeed.Flow ?? 0;
                double speed = hourlySpeed.Average;
                var flowAndSpeed = flow * speed;
                flowSpeed.Add(flowAndSpeed);
            }
            var totalFlowAndSpeed = flowSpeed.Sum();
            return (int)Math.Round(totalFlowAndSpeed / sumFlow);
        }

        private int GetWeigthtedEightyFifthAverageSpeed(List<HourlySpeed> hourlySpeeds)
        {
            if (hourlySpeeds == null || hourlySpeeds.Count() == 0)
            {
                return 0;
            }
            //IF we are doing 85th%ile AND source is ATSPM AND the flow is < 4 THEN ignore that row
            var filteredFlow = hourlySpeeds.Where(hourlySpeed => (!(hourlySpeed.Flow < 4 && hourlySpeed.SourceId == 1)));
            double sumFlow = filteredFlow.Sum(hs => hs.Flow ?? 0);
            //if flow is all zero do normal average
            if (sumFlow <= 0)
            {
                //var filteredEightyFifth = hourlySpeeds.Where(n => (!(n.EightyFifthSpeed == 0 || n.EightyFifthSpeed == null)));
                double totalAverage = hourlySpeeds.Sum(hs => hs.EightyFifthSpeed ?? 0);
                return (int)Math.Round(totalAverage / hourlySpeeds.Count());
            }
            var flowSpeed = new List<double>();
            foreach (var hourlySpeed in filteredFlow)
            {
                double flow = hourlySpeed.Flow ?? 0;
                double speed = hourlySpeed.EightyFifthSpeed ?? 0;
                var flowAndSpeed = flow * speed;
                flowSpeed.Add(flowAndSpeed);
            }
            var totalFlowAndSpeed = flowSpeed.Sum();
            return (int)Math.Round(totalFlowAndSpeed / sumFlow);
        }

    }
}
