using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class SplitFailAggregationByApproach : AggregationByApproach
    {
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;

        public SplitFailAggregationByApproach(
            Approach approach,
            ApproachSplitFailAggregationOptions splitFailAggregationOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            AggregationOptions options
            ) : base(approach, splitFailAggregationOptions, startDate, endDate,
            getProtectedPhase, dataType, options)
        {
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            LoadBins(approach, splitFailAggregationOptions, getProtectedPhase, dataType, options);
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (SplitFailDataTypes)dataType;
            var selectionEndDate = BinsContainers.Max(b => b.End);
            //Add a day so that it gets all the data for the entire end day instead of stoping at 12:00AM
            if (options.TimeOptions.SelectedBinSize == TimeOptions.BinSize.Day)
            {
                selectionEndDate = selectionEndDate.AddDays(1);
            }
            var splitFails =
                approachSplitFailAggregationRepository
                .GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End)
                .Where(a => a.ApproachId == approach.Id && a.IsProtectedPhase == getProtectedPhase)
                .ToList();

            if (splitFails != null)
            {
                var concurrentBinContainers = new ConcurrentBag<BinsContainer>();
                foreach (var binsContainer in BinsContainers)
                //Parallel.ForEach(BinsContainers, binsContainer =>
                {
                    var tempBinsContainer =
                        new BinsContainer(binsContainer.Start, binsContainer.End);
                    var concurrentBins = new ConcurrentBag<Bin>();
                    foreach (var bin in binsContainer.Bins)
                    //Parallel.ForEach(binsContainer.Bins, bin =>
                    {
                        if (splitFails.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var splitFailCount = 0;
                            switch (dataTypeEnum)
                            {
                                case SplitFailDataTypes.SplitFailures:
                                    splitFailCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SplitFailures);
                                    break;
                                case SplitFailDataTypes.GreenOccupancySum:
                                    splitFailCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GreenOccupancySum);
                                    break;
                                case SplitFailDataTypes.RedOccupancySum:
                                    splitFailCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.RedOccupancySum);
                                    break;
                                case SplitFailDataTypes.GreenTimeSum:
                                    splitFailCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.GreenTimeSum);
                                    break;
                                case SplitFailDataTypes.RedTimeSum:
                                    splitFailCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.RedTimeSum);
                                    break;
                                case SplitFailDataTypes.Cycles:
                                    splitFailCount =
                                        splitFails.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.Cycles);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Split Failure");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = splitFailCount,
                                Average = splitFailCount
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
                    }//);
                    tempBinsContainer.Bins = concurrentBins.OrderBy(c => c.Start).ToList();
                    concurrentBinContainers.Add(tempBinsContainer);
                }//);
                BinsContainers = concurrentBinContainers.OrderBy(b => b.Start).ToList();
            }
        }

    }
}