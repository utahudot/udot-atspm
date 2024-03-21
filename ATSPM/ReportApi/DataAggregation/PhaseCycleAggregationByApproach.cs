using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class PhaseCycleAggregationByApproach : AggregationByApproach
    {
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;

        public PhaseCycleAggregationByApproach(
            Approach approach,
            ApproachAggregationMetricOptions options,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            AggregatedDataType dataType,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository) : base(approach, options, startDate, endDate,
            getProtectedPhase, dataType)
        {
            LoadBins(approach, options, getProtectedPhase, dataType);
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
        }

        protected override void LoadBins(Approach approach, ApproachAggregationMetricOptions options,
            bool getProtectedPhase,
            AggregatedDataType dataType)
        {
            var approachCycles =
                phaseCycleAggregationRepository.GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End).Where(a => a.ApproachId == approach.Id);
            if (approachCycles != null)
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
                        if (approachCycles.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var approachCycleCount = 0;
                            switch (dataType.DataName)
                            {
                                case PhaseCycleAggregationOptions.TOTAL_RED_TO_RED_CYCLES:
                                    approachCycleCount =
                                        approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.TotalRedToRedCycles);
                                    break;
                                case PhaseCycleAggregationOptions.TOTAL_GREEN_TO_GREEN_CYCLES:
                                    approachCycleCount =
                                        approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.TotalGreenToGreenCycles);
                                    break;
                                case PhaseCycleAggregationOptions.RED_TIME:
                                    approachCycleCount =
                                        (int)approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.RedTime);
                                    break;
                                case PhaseCycleAggregationOptions.YELLOW_TIME:
                                    approachCycleCount =
                                        (int)approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.YellowTime);
                                    break;
                                case PhaseCycleAggregationOptions.GREEN_TIME:
                                    approachCycleCount =
                                        (int)approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GreenTime);
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