using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;
using System.Collections.Concurrent;

namespace MOE.Common.Business.DataAggregation
{
    public class YellowRedActivationsAggregationByApproach : AggregationByApproach
    {
        private readonly IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository;

        public YellowRedActivationsAggregationByApproach(
            Approach approach,
            ApproachYellowRedActivationsAggregationOptions approachYellowRedActivationsAggregationOptions,
            DateTime startDate,
            DateTime endDate,
            bool getProtectedPhase,
            int dataType,
            IApproachYellowRedActivationAggregationRepository approachYellowRedActivationAggregationRepository,
            AggregationOptions options
            ) : base(approach, approachYellowRedActivationsAggregationOptions, startDate, endDate,
            getProtectedPhase, dataType, options)
        {
            LoadBins(approach, approachYellowRedActivationsAggregationOptions, getProtectedPhase, dataType, options);
            this.approachYellowRedActivationAggregationRepository = approachYellowRedActivationAggregationRepository;
        }

        protected override void LoadBins(
            Approach approach,
            ApproachAggregationMetricOptions approachAggregationMetricOptions,
            bool getProtectedPhase,
            int dataType,
            AggregationOptions options
            )
        {
            var dataTypeEnum = (YellowRedActivationsDataTypes)dataType;
            var yellowRedActivations = approachYellowRedActivationAggregationRepository
                 .GetAggregationsBetweenDates(approach.Location.LocationIdentifier, options.Start, options.End)
                .Where(a => a.ApproachId == approach.Id && a.IsProtectedPhase == getProtectedPhase)
                .ToList();
            if (yellowRedActivations != null)
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
                        if (yellowRedActivations.Any(s => s.Start >= bin.Start && s.Start < bin.End))
                        {
                            var yellowRedActivationCount = 0;
                            switch (dataTypeEnum)
                            {
                                case YellowRedActivationsDataTypes.SevereRedLightViolations:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.SevereRedLightViolations);
                                    break;
                                case YellowRedActivationsDataTypes.TotalRedLightViolations:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.TotalRedLightViolations);
                                    break;
                                case YellowRedActivationsDataTypes.YellowActivations:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.YellowActivations);
                                    break;
                                case YellowRedActivationsDataTypes.ViolationTime:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.ViolationTime);
                                    break;
                                case YellowRedActivationsDataTypes.Cycles:
                                    yellowRedActivationCount =
                                        yellowRedActivations.Where(s =>
                                                s.Start >= bin.Start && s.Start < bin.End)
                                            .Sum(s => s.Cycles);
                                    break;
                                default:

                                    throw new Exception("Unknown Aggregate Data Type for Yellow Red Activation");
                            }

                            concurrentBins.Add(new Bin
                            {
                                Start = bin.Start,
                                End = bin.End,
                                Sum = yellowRedActivationCount,
                                Average = yellowRedActivationCount
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