using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public enum PedDataTypes
    {
        PedRequests,
        MaxPedDelay,
        MinPedDelay,
        PedDelaySum,
        PedCycles
    }
    //public const string PED_REQUESTS = "PedRequests";
    //public const string MAX_PED_DELAY = "MaxPedDelay";
    //public const string MIN_PED_DELAY = "MinPedDelay";
    //public const string PED_DELAY_SUM = "PedDelaySum";
    //public const string PED_CYCLES = "PedCycles";
    public class PhasePedAggregationByPhase : AggregationByPhase
    {

        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;

        public PhasePedAggregationByPhase(
            Location signal,
            int phaseNumber,
            PhasePedAggregationOptions phasePedAggregationOptions,
            int dataType,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            AggregationOptions options
            )
            : base(signal, phaseNumber, phasePedAggregationOptions, dataType, options)
        {
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            LoadBins(signal, phaseNumber, phasePedAggregationOptions, dataType, options);
        }

        protected override void LoadBins(Location signal, int phaseNumber, PhaseAggregationMetricOptions phaseAggregationMetricOptions,
            int dataType, AggregationOptions options)
        {
            var dataTypeEnum = (PedDataTypes)dataType;
            var pedAggs = phasePedAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, options.Start, options.End).Where(a => a.PhaseNumber == phaseNumber);
            if (pedAggs != null)
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
                        if (pedAggs.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var pedAggCount = 0;
                            switch (dataTypeEnum)
                            {
                                case PedDataTypes.PedCycles:
                                    pedAggCount =
                                        pedAggs.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.PedCycles);
                                    break;
                                case PedDataTypes.PedDelaySum:
                                    pedAggCount =
                                        pedAggs.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.PedDelay);
                                    break;
                                case PedDataTypes.MinPedDelay:
                                    pedAggCount =
                                        pedAggs.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.MinPedDelay);
                                    break;
                                case PedDataTypes.MaxPedDelay:
                                    pedAggCount =
                                        pedAggs.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.MaxPedDelay);
                                    break;
                                case PedDataTypes.PedRequests:
                                    pedAggCount =
                                        pedAggs.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.PedRequests);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Split Failure");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = pedAggCount,
                                Average = pedAggCount
                            });
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