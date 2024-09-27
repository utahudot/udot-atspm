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
            var expiredEvents = new TransformManyBlock<MonthlyAggregation, MonthlyAggregationProcessorDto>(GenerateTwoYearsForASegmentAsync);
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
                foreach (var source in segment.Entities)
                {
                    yield return new MonthlyAggregationProcessorDto
                    {
                        hourlySpeeds = new List<HourlySpeed>(),
                        startDate = firstDayOfPreviousMonth,
                        endDate = lastDayOfPreviousMonth,
                        SegmentId = segment.Id,
                        SourceId = (int)source.SourceId,
                        monthlyAggregation = new MonthlyAggregation
                        {
                            Id = Guid.NewGuid(),
                            CreatedDate = DateTime.SpecifyKind(today, DateTimeKind.Utc),
                            BinStartTime = firstDayOfPreviousMonth,
                            SegmentId = segment.Id,
                            //INSTEAD OF GET ALL SEGMENTS, DO GET SEGMENTS BY SOURCE ID, FOR KENZIE PROBABLY
                            SourceId = source.SourceId,
                            PercentObserved = 0.00
                        },
                        SpeedLimit = segment.SpeedLimit
                    };
                }

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
                    SourceId = source,
                    monthlyAggregation = new MonthlyAggregation
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.SpecifyKind(today, DateTimeKind.Utc),
                        BinStartTime = firstDayOfMonth,
                        SegmentId = segment.Id,
                        SourceId = source,
                        PercentObserved = 0.00
                    },
                    SpeedLimit = segment.SpeedLimit
                };
            }
        }

        private async IAsyncEnumerable<MonthlyAggregationProcessorDto> GenerateTwoYearsForASegmentAsync(MonthlyAggregation monthlyAggregation)
        {
            DateTime today = DateTime.Today;
            DateTime lastDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddDays(-1);
            DateTime firstDayOfPreviousMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            DateTime endCondition = new DateTime(today.Year, today.Month, 1).AddMonths(-1).AddYears(-2);
            var segment = await segmentRepository.LookupAsync(monthlyAggregation.SegmentId);

            while (firstDayOfPreviousMonth >= endCondition)
            {
                yield return new MonthlyAggregationProcessorDto
                {
                    hourlySpeeds = new List<HourlySpeed>(),
                    startDate = firstDayOfPreviousMonth,
                    endDate = lastDayOfPreviousMonth,
                    SegmentId = segment.Id,
                    SpeedLimit = segment.SpeedLimit,
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
            monthlyAggregationProcessor.hourlySpeeds = monthlyAggregationProcessor.hourlySpeeds.Where(i => i.SourceDataAnalyzed == true).ToList();
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForAllDay(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(0, 0, 0); // 12:00 AM
            TimeSpan endTime = new TimeSpan(23, 59, 59); // 11:59 PM
            var hourlySpeeds = monthlyAggregationProcessor.hourlySpeeds;

            monthlyAggregationProcessor.monthlyAggregation.PercentObserved = hourlySpeeds.Average(hs => hs.PercentObserved);

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.AllDayMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.AllDayPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.AllDayEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAllDayEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAllDayEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForOffPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(22, 0, 0); // 10:00 PM
            TimeSpan endTime = new TimeSpan(4, 0, 0); // 4:00 AM

            var aggregation = GetAveragesOfTimePeriodWithOvernightMetric(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriodWithOvernightMetric(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayOffPeakEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriodWithOvernightMetric(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendOffPeakEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForAmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(6, 0, 0); // 6:00 AM
            TimeSpan endTime = new TimeSpan(9, 0, 0);   // 9:00 AM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayAmPeakEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendAmPeakEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForPmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(16, 0, 0); // 4:00 PM
            TimeSpan endTime = new TimeSpan(18, 0, 0);   // 6:00 PM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayPmPeakEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendPmPeakEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForMidDay(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            TimeSpan endTime = new TimeSpan(16, 0, 0);   // 4:00 PM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.MidDayMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.MidDayPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.MidDayEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayMidDayEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendMidDayEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEvening(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(18, 0, 0); // 6:00 PM
            TimeSpan endTime = new TimeSpan(22, 0, 0);   // 10:00 PM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.EveningAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.EveningExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.EveningMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.EveningPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.EveningEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEveningEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEveningEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEarlyMorning(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(4, 0, 0); // 4:00 AM
            TimeSpan endTime = new TimeSpan(6, 0, 0);   // 6:00 AM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Total);

            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageSpeed = aggregation.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageEightyFifthSpeed = aggregation.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningViolations = aggregation.Violations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningExtremeViolations = aggregation.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningFlow = aggregation.Flow;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningMinSpeed = aggregation.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningMaxSpeed = aggregation.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningVariability = aggregation.Variability;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningPercentViolations = aggregation.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningPercentExtremeViolations = aggregation.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAvgSpeedVsSpeedLimit = aggregation.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningEightyFifthSpeedVsSpeedLimit = aggregation.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekday = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekday);

            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningAverageSpeed = aggregationWeekday.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningAverageEightyFifthSpeed = aggregationWeekday.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningViolations = aggregationWeekday.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningExtremeViolations = aggregationWeekday.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningFlow = aggregationWeekday.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningMinSpeed = aggregationWeekday.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningMaxSpeed = aggregationWeekday.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningVariability = aggregationWeekday.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningPercentViolations = aggregationWeekday.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningPercentExtremeViolations = aggregationWeekday.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningAvgSpeedVsSpeedLimit = aggregationWeekday.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekdayEarlyMorningEightyFifthSpeedVsSpeedLimit = aggregationWeekday.EightyFifthSpeedVsSpeedLimit;

            var aggregationWeekend = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit, DayType.Weekend);

            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningAverageSpeed = aggregationWeekend.AverageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningAverageEightyFifthSpeed = aggregationWeekend.AverageEightyFifthSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningViolations = aggregationWeekend.Violations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningExtremeViolations = aggregationWeekend.ExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningFlow = aggregationWeekend.Flow;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningMinSpeed = aggregationWeekend.MinSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningMaxSpeed = aggregationWeekend.MaxSpeed;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningVariability = aggregationWeekend.Variability;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningPercentViolations = aggregationWeekend.PercentViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningPercentExtremeViolations = aggregationWeekend.PercentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningAvgSpeedVsSpeedLimit = aggregationWeekend.AvgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.WeekendEarlyMorningEightyFifthSpeedVsSpeedLimit = aggregationWeekend.EightyFifthSpeedVsSpeedLimit;

            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationSimplified GetAveragesOfTimePeriodWithOvernightMetric(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime, long speedLimit, DayType dayType)
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
            if (dayType == DayType.Weekday)
            {
                filteredSpeeds = filteredSpeeds
                .Where(hs => hs.Date.DayOfWeek >= DayOfWeek.Monday && hs.Date.DayOfWeek <= DayOfWeek.Friday)
                .ToList();
            }
            else if (dayType == DayType.Weekend)
            {
                filteredSpeeds = filteredSpeeds
                .Where(hs => hs.Date.DayOfWeek == DayOfWeek.Saturday || hs.Date.DayOfWeek == DayOfWeek.Sunday)
                .ToList();
            }

            var aggregation = new MonthlyAggregationSimplified();
            var flow = filteredSpeeds.Sum(hs => hs.Flow ?? 0);
            aggregation.Flow = flow;
            double averageSpeed = GetWeigthtedAverageSpeed(filteredSpeeds);
            aggregation.AverageSpeed = averageSpeed;
            double eightyFifthSpeed = GetWeigthtedEightyFifthAverageSpeed(filteredSpeeds);
            aggregation.AverageEightyFifthSpeed = eightyFifthSpeed;
            var totalViolations = filteredSpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            aggregation.Violations = totalViolations;
            var totalExtremeViolations = filteredSpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            aggregation.ExtremeViolations = totalExtremeViolations;

            double? min = filteredSpeeds.Min(hs => hs.MinSpeed);
            aggregation.MinSpeed = min;
            double? max = filteredSpeeds.Max(hs => hs.MaxSpeed);
            aggregation.MaxSpeed = max;
            double? variability = filteredSpeeds.Count > 0 ? (((double)filteredSpeeds.Max(h => h.MaxSpeed ?? 0)) - ((double)filteredSpeeds.Min(h => h.MinSpeed ?? 0))) : (double?)null;
            aggregation.Variability = variability;
            double percentViolations = (flow != 0 ? ((double)totalViolations / flow) : 0) * 100;
            aggregation.PercentViolations = percentViolations;
            double percentExtremeViolations = (flow != 0 ? ((double)totalExtremeViolations / flow) : 0) * 100;
            aggregation.PercentExtremeViolations = percentExtremeViolations;
            double? avgSpeedVsSpeedLimit = speedLimit != 0 ? (double)averageSpeed - speedLimit : (double?)null;
            aggregation.AvgSpeedVsSpeedLimit = avgSpeedVsSpeedLimit;
            double? avgEightyFifthSpeedLimit = speedLimit != 0 ? (double)eightyFifthSpeed - speedLimit : (double?)null;
            aggregation.EightyFifthSpeedVsSpeedLimit = avgEightyFifthSpeedLimit;

            return aggregation;
        }

        private MonthlyAggregationSimplified GetAveragesOfTimePeriod(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime, long speedLimit, DayType dayType)
        {
            if (hourlySpeeds.Count == 0)
            {
                return new MonthlyAggregationSimplified();
            }
            var filteredByTime = hourlySpeeds
                .Where(hs => hs.BinStartTime.TimeOfDay >= startTime && hs.BinStartTime.TimeOfDay <= endTime)
                .ToList();
            if (filteredByTime.Count == 0)
            {
                return new MonthlyAggregationSimplified();
            }

            // Find the minimum and maximum dates in the filtered list
            var minDate = filteredByTime.Min(hs => hs.Date);
            var maxDate = filteredByTime.Max(hs => hs.Date);

            // Filter out the first and last dates
            var filteredSpeeds = filteredByTime
                .Where(hs => hs.Date != minDate && hs.Date != maxDate)
                .ToList();
            if (dayType == DayType.Weekday)
            {
                filteredSpeeds = filteredSpeeds
                .Where(hs => hs.Date.DayOfWeek >= DayOfWeek.Monday && hs.Date.DayOfWeek <= DayOfWeek.Friday)
                .ToList();
            }
            else if (dayType == DayType.Weekend)
            {
                filteredSpeeds = filteredSpeeds
                .Where(hs => hs.Date.DayOfWeek == DayOfWeek.Saturday || hs.Date.DayOfWeek == DayOfWeek.Sunday)
                .ToList();
            }

            var aggregation = new MonthlyAggregationSimplified();
            var flow = filteredSpeeds.Sum(hs => hs.Flow ?? 0);
            aggregation.Flow = flow;
            double averageSpeed = GetWeigthtedAverageSpeed(filteredSpeeds);
            aggregation.AverageSpeed = averageSpeed;
            double eightyFifthSpeed = GetWeigthtedEightyFifthAverageSpeed(filteredSpeeds);
            aggregation.AverageEightyFifthSpeed = eightyFifthSpeed;
            var totalViolations = filteredSpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            aggregation.Violations = totalViolations;
            var totalExtremeViolations = filteredSpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            aggregation.ExtremeViolations = totalExtremeViolations;

            double? min = filteredSpeeds.Min(hs => hs.MinSpeed);
            aggregation.MinSpeed = min;
            double? max = filteredSpeeds.Max(hs => hs.MaxSpeed);
            aggregation.MaxSpeed = max;
            double? variability = filteredSpeeds.Count > 0 ? (((double)filteredSpeeds.Max(h => h.MaxSpeed ?? 0)) - ((double)filteredSpeeds.Min(h => h.MinSpeed ?? 0))) : (double?)null;
            aggregation.Variability = variability;
            double percentViolations = (flow != 0 ? ((double)totalViolations / flow) : 0) * 100;
            aggregation.PercentViolations = percentViolations;
            double percentExtremeViolations = (flow != 0 ? ((double)totalExtremeViolations / flow) : 0) * 100;
            aggregation.PercentExtremeViolations = percentExtremeViolations;
            double? avgSpeedVsSpeedLimit = speedLimit != 0 ? (double)averageSpeed - speedLimit : (double?)null;
            aggregation.AvgSpeedVsSpeedLimit = avgSpeedVsSpeedLimit;
            double? avgEightyFifthSpeedLimit = speedLimit != 0 ? (double)eightyFifthSpeed - speedLimit : (double?)null;
            aggregation.EightyFifthSpeedVsSpeedLimit = avgEightyFifthSpeedLimit;

            return aggregation;
        }

        private double GetWeigthtedAverageSpeed(List<HourlySpeed> hourlySpeeds)
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
                return (double)totalAverage / hourlySpeeds.Count();
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
            return (double)totalFlowAndSpeed / sumFlow;
        }

        private double GetWeigthtedEightyFifthAverageSpeed(List<HourlySpeed> hourlySpeeds)
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
                return (double)totalAverage / hourlySpeeds.Count();
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
            return (double)totalFlowAndSpeed / sumFlow;
        }

    }
}
