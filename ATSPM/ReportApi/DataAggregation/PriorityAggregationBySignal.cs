using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class PriorityAggregationBySignal : AggregationBySignal
    {
        private readonly IPriorityAggregationRepository priorityAggregationRepository;

        public PriorityAggregationBySignal(
            SignalAggregationMetricOptions signalAggregationMetricOptions,
            Location signal,
            IPriorityAggregationRepository priorityAggregationRepository,
            AggregationOptions options
            ) : base(signalAggregationMetricOptions,
            signal, options)
        {
            this.priorityAggregationRepository = priorityAggregationRepository;
            LoadBins(signalAggregationMetricOptions, signal, options);
        }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            var dataTypeEnum = (PriorityDataTypes)options.DataType;
            var selectionEndDate = BinsContainers.Max(b => b.End);
            //Add a day so that it gets all the data for the entire end day instead of stoping at 12:00AM
            if (options.TimeOptions.SelectedBinSize == TimeOptions.BinSize.Day)
            {
                selectionEndDate = selectionEndDate.AddDays(1);
            }
            var priorityAggregations = priorityAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, BinsContainers.Min(b => b.Start), selectionEndDate).ToList();
            if (priorityAggregations != null)
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
                        if (priorityAggregations.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var preemptionSum = 0;

                            switch (dataTypeEnum)
                            {
                                case PriorityDataTypes.PriorityNumber:
                                    preemptionSum = priorityAggregations.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PriorityNumber);
                                    break;
                                case PriorityDataTypes.PriorityRequests:
                                    preemptionSum = priorityAggregations.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PriorityRequests);
                                    break;
                                case PriorityDataTypes.PriorityServiceEarlyGreen:
                                    preemptionSum = priorityAggregations.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PriorityServiceEarlyGreen);
                                    break;
                                case PriorityDataTypes.PriorityServiceExtendedGreen:
                                    preemptionSum = priorityAggregations.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.PriorityServiceEarlyGreen);
                                    break;
                                default:
                                    throw new Exception("Invalid Aggregate Data Type");
                            }

                            Bin newBin = new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = preemptionSum,
                                Average = preemptionSum
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

        protected override void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            throw new NotImplementedException();
        }
    }
}