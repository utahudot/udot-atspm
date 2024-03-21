//using ATSPM.Application.Business.Common;
//using ATSPM.Application.Enums;
//using ATSPM.Application.Repositories.AggregationRepositories;
//using ATSPM.Data.Models.AggregationModels;
//using ATSPM.Domain.Common;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace ATSPM.Application.Business.Aggregation
//{


//    public class AggregationEngineService
//    {
//        private readonly IDictionary<AggregationType, object> _repositories;

//        public AggregationEngineService(
//            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
//            IApproachPcdAggregationRepository approachPcdAggregationRepository,
//            IApproachSpeedAggregationRepository approachSpeedAggregationRepository,
//            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
//            IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository,
//            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
//            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
//            IPhasePedAggregationRepository phasePedAggregationRepository,
//            IPhaseSplitMonitorAggregationRepository phaseSplitMonitorAggregationRepository,
//            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
//            IPreemptionAggregationRepository preemptionAggregationRepository,
//            IPriorityAggregationRepository priorityAggregationRepository,
//            ISignalEventCountAggregationRepository signalEventCountAggregationRepository,
//            ISignalPlanAggregationRepository signalPlanAggregationRepository)
//        {
//            _repositories = new Dictionary<AggregationType, object>
//            {
//                { AggregationType.DetectorEventCount, detectorEventCountAggregationRepository },
//                { AggregationType.Pcd, approachPcdAggregationRepository },
//                { AggregationType.Speed, approachSpeedAggregationRepository },
//                { AggregationType.SplitFail, approachSplitFailAggregationRepository },
//                { AggregationType.YellowRedActivation, approachYellowRedActivationAggregationRepository },
//                { AggregationType.PhaseCycle, phaseCycleAggregationRepository },
//                { AggregationType.PhaseLeftTurn, phaseLeftTurnGapAggregationRepository },
//                { AggregationType.Ped, phasePedAggregationRepository },
//                { AggregationType.SplitMonitor, phaseSplitMonitorAggregationRepository },
//                { AggregationType.PhaseTermination, phaseTerminationAggregationRepository },
//                { AggregationType.Preemption, preemptionAggregationRepository },
//                { AggregationType.Priority, priorityAggregationRepository },
//                { AggregationType.SignalEventCount, signalEventCountAggregationRepository },
//                { AggregationType.signalPlan, signalPlanAggregationRepository }
//            };
//        }

//        public IEnumerable<AggregationResult> AggregateData<T, TKey>(
//            AggregationType aggregationType,
//            Func<T, TKey> keySelector,
//            Func<T, bool> filterFunc,
//            List<string> locationIdentifiers,
//            DateTime start,
//            DateTime end,
//            TimeSpan binSize,
//            Func<IEnumerable<T>, double> aggregateFunc) where T : AggregationModelBase, new()
//        {
//            var allData = new List<T>();

//            var repository = GetRepository<T>(aggregationType);

//            foreach (var locationIdentifier in locationIdentifiers)
//            {
//                var data = repository.GetAggregationsBetweenDates(locationIdentifier, start, end);
//                allData.AddRange(data);
//            }
//            var filteredData = allData.Where(filterFunc);
//            var segmentedData = SegmentData(filteredData, keySelector);
//            return AggregateSegmentedData(segmentedData, start, end, binSize, aggregateFunc);
//        }

//        private IAggregationRepository<T> GetRepository<T>(AggregationType aggregationType) where T : AggregationModelBase
//        {
//            if (_repositories.TryGetValue(aggregationType, out var repository) && repository is IAggregationRepository<T> typedRepository)
//            {
//                return typedRepository;
//            }

//            throw new ArgumentException($"Repository for the given type {typeof(T).Name} and aggregation type {aggregationType} not found.");
//        }

//        private IEnumerable<IGrouping<TKey, T>> SegmentData<T, TKey>(
//            IEnumerable<T> data,
//            Func<T, TKey> keySelector)
//        {
//            return data.GroupBy(keySelector);
//        }

//        private IEnumerable<AggregationResult> AggregateSegmentedData<T, TKey>(
//        IEnumerable<IGrouping<TKey, T>> segmentedData,
//        DateTime start,
//        DateTime end,
//        TimeSpan binSize,
//        Func<IEnumerable<T>, double> aggregateFunc) where T : IStartEndRange, new()
//        {
//            var aggregatedData = new List<AggregationResult>();
//            var timeline = new Timeline<T>(start, end, binSize);

//            foreach (var segment in segmentedData)
//            {
//                var aggregateResult = new AggregationResult
//                {
//                    Start = start,
//                    End = end,
//                    Identifier = segment.Key.ToString(),
//                    Summary = 0,
//                    DataPoints = new List<DataPointForDouble>()
//                };
//                foreach (var timeSegment in timeline.Segments)
//                {
//                    var segmentData = segment.Where(d => timeSegment.InRange(d.Start) && timeSegment.InRange(d.End));
//                    var result = aggregateFunc(segmentData);
//                    aggregateResult.DataPoints.Add(new DataPointForDouble(timeSegment.Start, result));
//                }
//                aggregatedData.Add(aggregateResult);
//            }

//            return aggregatedData;
//        }
//    }


//}

