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

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AllDayAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.AllDayViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.AllDayMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.AllDayMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.AllDayVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.AllDayPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AllDayAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.AllDayEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForOffPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(22, 0, 0); // 10:00 PM
            TimeSpan endTime = new TimeSpan(4, 0, 0); // 4:00 AM

            var aggregation = GetAveragesOfTimePeriodWithOvernightMetric(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.OffPeakMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.OffPeakEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForAmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(6, 0, 0); // 6:00 AM
            TimeSpan endTime = new TimeSpan(9, 0, 0);   // 9:00 AM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.AmPeakMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.AmPeakEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForPmPeak(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(16, 0, 0); // 4:00 PM
            TimeSpan endTime = new TimeSpan(18, 0, 0);   // 6:00 PM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.PmPeakMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.PmPeakEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForMidDay(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            TimeSpan endTime = new TimeSpan(16, 0, 0);   // 4:00 PM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.MidDayAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.MidDayViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.MidDayMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.MidDayMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.MidDayVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.MidDayPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.MidDayAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.MidDayEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEvening(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(18, 0, 0); // 6:00 PM
            TimeSpan endTime = new TimeSpan(22, 0, 0);   // 10:00 PM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.EveningAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EveningAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.EveningViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.EveningMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.EveningMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.EveningVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.EveningPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EveningAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.EveningEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private MonthlyAggregationProcessorDto GetHourlySpeedsForEarlyMorning(MonthlyAggregationProcessorDto monthlyAggregationProcessor)
        {
            TimeSpan startTime = new TimeSpan(4, 0, 0); // 4:00 AM
            TimeSpan endTime = new TimeSpan(6, 0, 0);   // 6:00 AM

            var aggregation = GetAveragesOfTimePeriod(monthlyAggregationProcessor.hourlySpeeds, startTime, endTime, monthlyAggregationProcessor.SpeedLimit);

            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageSpeed = aggregation.averageSpeed;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAverageEightyFifthSpeed = aggregation.eightyFifthAverage;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningViolations = aggregation.totalViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningExtremeViolations = aggregation.totalExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningFlow = aggregation.flow;

            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningMinSpeed = aggregation.min;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningMaxSpeed = aggregation.max;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningVariability = aggregation.variability;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningPercentViolations = aggregation.percentViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningPercentExtremeViolations = aggregation.percentExtremeViolations;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningAvgSpeedVsSpeedLimit = aggregation.avgSpeedVsSpeedLimit;
            monthlyAggregationProcessor.monthlyAggregation.EarlyMorningEightyFifthSpeedVsSpeedLimit = aggregation.eightyFifthSpeedVsSpeedLimit;
            return monthlyAggregationProcessor;
        }

        private AggregationsOverTimePeriod GetAveragesOfTimePeriodWithOvernightMetric(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime, long speedLimit)
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

            var aggregation = new AggregationsOverTimePeriod();
            var flow = filteredSpeeds.Sum(hs => hs.Flow ?? 0);
            aggregation.flow = flow;
            double averageSpeed = GetWeigthtedAverageSpeed(filteredSpeeds);
            aggregation.averageSpeed = averageSpeed;
            double eightyFifthSpeed = GetWeigthtedEightyFifthAverageSpeed(filteredSpeeds);
            aggregation.eightyFifthAverage = eightyFifthSpeed;
            var totalViolations = filteredSpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            aggregation.totalViolations = totalViolations;
            var totalExtremeViolations = filteredSpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            aggregation.totalExtremeViolations = totalExtremeViolations;

            double? min = filteredSpeeds.Min(hs => hs.MinSpeed);
            aggregation.min = min;
            double? max = filteredSpeeds.Max(hs => hs.MaxSpeed);
            aggregation.max = max;
            double? variability = filteredSpeeds.Count > 0 ? (((double)filteredSpeeds.Max(h => h.MaxSpeed ?? 0)) - ((double)filteredSpeeds.Min(h => h.MinSpeed ?? 0))) : (double?)null;
            aggregation.variability = variability;
            double percentViolations = (flow != 0 ? ((double)totalViolations / flow) : 0) * 100;
            aggregation.percentViolations = percentViolations;
            double percentExtremeViolations = (flow != 0 ? ((double)totalExtremeViolations / flow) : 0) * 100;
            aggregation.percentExtremeViolations = percentExtremeViolations;
            double? avgSpeedVsSpeedLimit = speedLimit != 0 ? (double)averageSpeed - speedLimit : (double?)null;
            aggregation.avgSpeedVsSpeedLimit = avgSpeedVsSpeedLimit;
            double? avgEightyFifthSpeedLimit = speedLimit != 0 ? (double)eightyFifthSpeed - speedLimit : (double?)null;
            aggregation.eightyFifthSpeedVsSpeedLimit = avgEightyFifthSpeedLimit;

            return aggregation;
        }

        private AggregationsOverTimePeriod GetAveragesOfTimePeriod(List<HourlySpeed> hourlySpeeds, TimeSpan startTime, TimeSpan endTime, long speedLimit)
        {
            if (hourlySpeeds.Count == 0)
            {
                return (0, 0, 0, 0, 0, null, null, 0, 0, 0, 0, 0);
            }
            var filteredByTime = hourlySpeeds
                .Where(hs => hs.BinStartTime.TimeOfDay >= startTime && hs.BinStartTime.TimeOfDay <= endTime)
                .ToList();
            if (filteredByTime.Count == 0)
            {
                return (0, 0, 0, 0, 0, null, null, 0, 0, 0, 0, 0);
            }

            // Find the minimum and maximum dates in the filtered list
            var minDate = filteredByTime.Min(hs => hs.Date);
            var maxDate = filteredByTime.Max(hs => hs.Date);

            // Filter out the first and last dates
            var filteredSpeeds = filteredByTime
                .Where(hs => hs.Date != minDate && hs.Date != maxDate)
                .ToList();

            var aggregation = new AggregationsOverTimePeriod();
            var flow = filteredSpeeds.Sum(hs => hs.Flow ?? 0);
            aggregation.flow = flow;
            double averageSpeed = GetWeigthtedAverageSpeed(filteredSpeeds);
            aggregation.averageSpeed = averageSpeed;
            double eightyFifthSpeed = GetWeigthtedEightyFifthAverageSpeed(filteredSpeeds);
            aggregation.eightyFifthAverage = eightyFifthSpeed;
            var totalViolations = filteredSpeeds.Sum(hs => hs.Violation.GetValueOrDefault());
            aggregation.totalViolations = totalViolations;
            var totalExtremeViolations = filteredSpeeds.Sum(hs => hs.ExtremeViolation.GetValueOrDefault());
            aggregation.totalExtremeViolations = totalExtremeViolations;

            double? min = filteredSpeeds.Min(hs => hs.MinSpeed);
            aggregation.min = min;
            double? max = filteredSpeeds.Max(hs => hs.MaxSpeed);
            aggregation.max = max;
            double? variability = filteredSpeeds.Count > 0 ? (((double)filteredSpeeds.Max(h => h.MaxSpeed ?? 0)) - ((double)filteredSpeeds.Min(h => h.MinSpeed ?? 0))) : (double?)null;
            aggregation.variability = variability;
            double percentViolations = (flow != 0 ? ((double)totalViolations / flow) : 0) * 100;
            aggregation.percentViolations = percentViolations;
            double percentExtremeViolations = (flow != 0 ? ((double)totalExtremeViolations / flow) : 0) * 100;
            aggregation.percentExtremeViolations = percentExtremeViolations;
            double? avgSpeedVsSpeedLimit = speedLimit != 0 ? (double)averageSpeed - speedLimit : (double?)null;
            aggregation.avgSpeedVsSpeedLimit = avgSpeedVsSpeedLimit;
            double? avgEightyFifthSpeedLimit = speedLimit != 0 ? (double)eightyFifthSpeed - speedLimit : (double?)null;
            aggregation.eightyFifthSpeedVsSpeedLimit = avgEightyFifthSpeedLimit;

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

    internal record struct AggregationsOverTimePeriod(double averageSpeed, long totalViolations, long totalExtremeViolations, double eightyFifthAverage, long flow, double? min, double? max, double? variability, double percentViolations, double percentExtremeViolations, double? avgSpeedVsSpeedLimit, double? eightyFifthSpeedVsSpeedLimit)
    {
        public static implicit operator (double averageSpeed, long totalViolations, long totalExtremeViolations, double eightyFifthAverage, long flow, double? min, double? max, double? variability, double percentViolations, double percentExtremeViolations, double? avgSpeedVsSpeedLimit, double? eightyFifthSpeedVsSpeedLimit)(AggregationsOverTimePeriod value)
        {
            return (value.averageSpeed, value.totalViolations, value.totalExtremeViolations, value.eightyFifthAverage, value.flow, value.min, value.max, value.variability, value.percentViolations, value.percentExtremeViolations, value.avgSpeedVsSpeedLimit, value.eightyFifthSpeedVsSpeedLimit);
        }

        public static implicit operator AggregationsOverTimePeriod((double averageSpeed, long totalViolations, long totalExtremeViolations, double EightyFifthAverage, long flow, double? min, double? max, double? variability, double percentViolations, double percentExtremeViolations, double? avgSpeedVsSpeedLimit, double? eightyFifthSpeedVsSpeedLimit) value)
        {
            return new AggregationsOverTimePeriod(value.averageSpeed, value.totalViolations, value.totalExtremeViolations, value.EightyFifthAverage, value.flow, value.min, value.max, value.variability, value.percentViolations, value.percentExtremeViolations, value.avgSpeedVsSpeedLimit, value.eightyFifthSpeedVsSpeedLimit);
        }
    }
}
