using ATSPM.Application.Business.Aggregation;
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
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            AggregationOptions options
            ) : base(approach, approachAggregationMetricOptions, startDate, endDate,
            getProtectedPhase, dataType, options)
        {
            LoadBins(approach, approachAggregationMetricOptions, getProtectedPhase, dataType, options);
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options
            )
        {
            var approachCycles =
                phaseCycleAggregationRepository.GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End).Where(a => a.ApproachId == approach.Id);
            if (approachCycles != null)
            {
                var dataTypeEnum = (PhaseCycleDataTypes)dataType;
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
                            switch (dataTypeEnum)
                            {
                                case PhaseCycleDataTypes.TotalRedToRedCycles:
                                    approachCycleCount =
                                        approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.TotalRedToRedCycles);
                                    break;
                                case PhaseCycleDataTypes.TotalGreenToGreenCycles:
                                    approachCycleCount =
                                        approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.TotalGreenToGreenCycles);
                                    break;
                                case PhaseCycleDataTypes.RedTime:
                                    approachCycleCount =
                                        (int)approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.RedTime);
                                    break;
                                case PhaseCycleDataTypes.YellowTime:
                                    approachCycleCount =
                                        (int)approachCycles.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.YellowTime);
                                    break;
                                case PhaseCycleDataTypes.GreenTime:
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