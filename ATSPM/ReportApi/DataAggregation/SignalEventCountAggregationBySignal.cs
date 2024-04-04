using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class SignalEventCountAggregationBySignal : AggregationBySignal
    {
        private readonly ISignalEventCountAggregationRepository signalEventCountAggregationRepository;

        public SignalEventCountAggregationBySignal(
            SignalEventCountAggregationOptions signalEventCountAggregation,
            Location signal,
            ISignalEventCountAggregationRepository signalEventCountAggregationRepository,
            AggregationOptions options
            ) : base(
            signalEventCountAggregation, signal, options)
        {
            this.signalEventCountAggregationRepository = signalEventCountAggregationRepository;
            LoadBins(signalEventCountAggregation, signal, options);
        }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            var DataTypeEnum = (SignalEventCountDataTypes)options.DataType;
            var selectionEndDate = BinsContainers.Max(b => b.End);
            //Add a day so that it gets all the data for the entire end day instead of stoping at 12:00AM
            if (options.TimeOptions.SelectedBinSize == TimeOptions.BinSize.Day)
            {
                selectionEndDate = selectionEndDate.AddDays(1);
            }
            var signalEventCounts = signalEventCountAggregationRepository.GetAggregationsBetweenDates(signal.LocationIdentifier, BinsContainers.Min(b => b.Start), selectionEndDate).ToList();
            if (signalEventCounts != null)
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
                        if (signalEventCounts.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var signalEventCountSum = 0;

                            switch (DataTypeEnum)
                            {
                                case SignalEventCountDataTypes.EventCount:
                                    signalEventCountSum = signalEventCounts.Where(s =>
                                            s.Start >= bin.Start && s.Start < bin.End)
                                        .Sum(s => s.EventCount);
                                    break;
                            }
                            Bin newBin = new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = signalEventCountSum,
                                Average = signalEventCountSum
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
            throw new System.NotImplementedException();
        }
    }
}