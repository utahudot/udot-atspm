using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class PhaseLeftTurnGapAggregationByApproach : AggregationByApproach
    {
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;

        public PhaseLeftTurnGapAggregationByApproach(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            AggregationOptions options
            ) : base(approach, approachAggregationMetricOptions, startDate, endDate, getProtectedPhase, dataType, options)

        {
            LoadBins(approach, approachAggregationMetricOptions, getProtectedPhase, dataType, options);
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options)
        {
            var dataTypeEnum = (LeftTurnGapDataTypes)dataType;
            var approachLeftTurnGaps = phaseLeftTurnGapAggregationRepository.GetAggregationsBetweenDates(
                    approach.Location.LocationIdentifier, options.Start, options.End);
            if (approachLeftTurnGaps != null)
            {
                var concurrentBinContainers = new ConcurrentBag<BinsContainer>();
                //foreach (var binsContainer in binsContainers)
                Parallel.ForEach(BinsContainers, binsContainer =>
                {
                    var tempBinsContainer =
                        new BinsContainer(binsContainer.Start, binsContainer.End);
                    var concurrentBins = new ConcurrentBag<Bin>();
                    //foreach (var bin in binsContainer.Bins)
                    Parallel.ForEach(binsContainer.Bins, bin =>
                    {
                        if (approachLeftTurnGaps.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var approachCycleCount = 0.0;
                            switch (dataTypeEnum)
                            {
                                case LeftTurnGapDataTypes.GapCount1:
                                    approachCycleCount =
                                        approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount1);
                                    break;
                                case LeftTurnGapDataTypes.GapCount2:
                                    approachCycleCount =
                                        approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount2);
                                    break;
                                case LeftTurnGapDataTypes.GapCount3:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount3);
                                    break;
                                case LeftTurnGapDataTypes.GapCount4:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount4);
                                    break;
                                case LeftTurnGapDataTypes.GapCount5:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount5);
                                    break;
                                case LeftTurnGapDataTypes.GapCount6:
                                    approachCycleCount =
                                        approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount6);
                                    break;
                                case LeftTurnGapDataTypes.GapCount7:
                                    approachCycleCount =
                                        approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount7);
                                    break;
                                case LeftTurnGapDataTypes.GapCount8:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                               s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount8);
                                    break;
                                case LeftTurnGapDataTypes.GapCount9:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                               s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount9);
                                    break;
                                case LeftTurnGapDataTypes.GapCount10:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                               s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount10);
                                    break;
                                case LeftTurnGapDataTypes.GapCount11:
                                    approachCycleCount =
                                        approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GapCount11);
                                    break;
                                case LeftTurnGapDataTypes.SumGapDuration1:
                                    approachCycleCount =
                                        approachLeftTurnGaps.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SumGapDuration1);
                                    break;
                                case LeftTurnGapDataTypes.SumGapDuration2:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                               s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SumGapDuration2);
                                    break;
                                case LeftTurnGapDataTypes.SumGapDuration3:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                               s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SumGapDuration3);
                                    break;
                                case LeftTurnGapDataTypes.SumGreenTime:
                                    approachCycleCount =
                                        (int)approachLeftTurnGaps.Where(s =>
                                               s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SumGreenTime);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Approach Cycle");
                            }
                            Bin newBin = new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = approachCycleCount,
                                Average = approachCycleCount
                            };
                            concurrentBins.Add(newBin);
                        }
                        else
                        {
                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = 0,
                                Average = 0
                            });
                        }
                    });
                    tempBinsContainer.Bins = concurrentBins.OrderBy(c => c.Start).ToList();
                    concurrentBinContainers.Add(tempBinsContainer);
                });
                BinsContainers = concurrentBinContainers.OrderBy(b => b.Start).ToList();
            }
        }

    }
}