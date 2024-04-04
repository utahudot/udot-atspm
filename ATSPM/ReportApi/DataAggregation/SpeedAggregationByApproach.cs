using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class SpeedAggregationByApproach : AggregationByApproach
    {
        private readonly IApproachSpeedAggregationRepository approachSpeedAggregationRepository;

        public SpeedAggregationByApproach(
            Approach approach,
            ApproachSpeedAggregationOptions approachSpeedAggregationOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IApproachSpeedAggregationRepository approachSpeedAggregationRepository,
            AggregationOptions options
            ) : base(approach, approachSpeedAggregationOptions, startDate, endDate,
            getProtectedPhase, dataType, options)
        {
            this.approachSpeedAggregationRepository = approachSpeedAggregationRepository;
            LoadBins(approach, approachSpeedAggregationOptions, getProtectedPhase, dataType, options);
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options)
        {
            var dataTypeEnum = (SpeedAggregationDataTypes)dataType;
            var speedAggregations =
                approachSpeedAggregationRepository
                .GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End)
                .Where(a => a.ApproachId == approach.Id)
                .ToList();
            if (speedAggregations != null)
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
                        if (speedAggregations.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            double speedAggregationCount = 0;
                            switch (dataTypeEnum)
                            {
                                case SpeedAggregationDataTypes.AverageSpeed:
                                    if (speedAggregations.Any(s =>
                                        s.Start >= bin.Start && s.Start < bin.End))
                                    {
                                        double summedSpeed = speedAggregations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SummedSpeed);
                                        double summedVolume = speedAggregations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                                .Sum(s => s.SpeedVolume);
                                        if (summedVolume > 0)
                                            speedAggregationCount = summedSpeed / summedVolume;
                                    }
                                    break;
                                case SpeedAggregationDataTypes.SpeedVolume:
                                    speedAggregationCount =
                                        speedAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SpeedVolume);
                                    break;
                                case SpeedAggregationDataTypes.Percentile85Speed:
                                    speedAggregationCount =
                                        speedAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.Speed85th);
                                    break;
                                case SpeedAggregationDataTypes.Percentile15Speed:
                                    speedAggregationCount =
                                        speedAggregations.Where(s => s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.Speed15th);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Split Failure");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = Convert.ToInt32(Math.Round(speedAggregationCount)),
                                Average = speedAggregationCount
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