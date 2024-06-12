#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/SpeedAggregationBySignal.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using MOE.Common.Business.WCFServiceLibrary;

namespace MOE.Common.Business.DataAggregation
{
    public class SpeedAggregationBySignal : AggregationBySignal
    {
        private readonly IApproachSpeedAggregationRepository approachSpeedAggregationRepository;

        public SpeedAggregationBySignal(
            ApproachSpeedAggregationOptions approachSpeedAggregationOptions,
            Location signal,
            IApproachSpeedAggregationRepository approachSpeedAggregationRepository,
            AggregationOptions options
            ) : base(
            approachSpeedAggregationOptions, signal, options)
        {
            this.approachSpeedAggregationRepository = approachSpeedAggregationRepository;
            ApproachSpeedEvents = new List<SpeedAggregationByApproach>();
            GetApproachSpeedEventsAggregationContainersForAllApporaches(approachSpeedAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public SpeedAggregationBySignal(
            ApproachSpeedAggregationOptions approachSpeedAggregationOptions,
            Location signal,
            int phaseNumber,
            IApproachSpeedAggregationRepository approachSpeedAggregationRepository,
            AggregationOptions options
            ) : base(approachSpeedAggregationOptions, signal, options)
        {
            this.approachSpeedAggregationRepository = approachSpeedAggregationRepository;
            ApproachSpeedEvents = new List<SpeedAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachSpeedEvents.Add(
                        new SpeedAggregationByApproach(
                            approach,
                            approachSpeedAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachSpeedAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachSpeedEvents.Add(
                            new SpeedAggregationByApproach(
                                approach,
                                approachSpeedAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachSpeedAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public SpeedAggregationBySignal(
            ApproachSpeedAggregationOptions approachSpeedAggregationOptions,
            Location signal,
            DirectionTypes direction,
            AggregationOptions options
            ) : base(approachSpeedAggregationOptions, signal, options)
        {
            ApproachSpeedEvents = new List<SpeedAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachSpeedEvents.Add(
                        new SpeedAggregationByApproach(
                            approach,
                            approachSpeedAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachSpeedAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachSpeedEvents.Add(
                            new SpeedAggregationByApproach(
                                approach,
                                approachSpeedAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachSpeedAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public List<SpeedAggregationByApproach> ApproachSpeedEvents { get; }

        protected override void LoadBins(SignalAggregationMetricOptions aggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var speedAggregationByApproach in ApproachSpeedEvents)
                        bin.Sum += speedAggregationByApproach.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachSpeedEvents.Count > 0 ? bin.Sum / ApproachSpeedEvents.Count : 0;
                }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var speedAggregationByApproach in ApproachSpeedEvents)
                        bin.Sum += speedAggregationByApproach.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachSpeedEvents.Count > 0 ? bin.Sum / ApproachSpeedEvents.Count : 0;
                }
        }

        private void GetApproachSpeedEventsAggregationContainersForAllApporaches(
            ApproachSpeedAggregationOptions approachSpeedAggregationOptions,
            Location signal,
            AggregationOptions options
            )
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachSpeedEvents.Add(
                    new SpeedAggregationByApproach(
                        approach,
                        approachSpeedAggregationOptions,
                        options.Start,
                        options.End,
                        true,
                        options.DataType,
                        approachSpeedAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachSpeedEvents.Add(
                        new SpeedAggregationByApproach(
                            approach,
                            approachSpeedAggregationOptions,
                            options.Start,
                            options.End,
                            false,
                            options.DataType,
                            approachSpeedAggregationRepository,
                            options
                            ));
            }
        }


        public double GetSpeedEventssByDirection(DirectionTypes direction)
        {
            double speedEvents = 0;
            if (ApproachSpeedEvents != null)
                speedEvents = ApproachSpeedEvents
                    .Where(a => a.Approach.DirectionType.Id == direction)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return speedEvents;
        }

        public int GetAverageSpeedEventssByDirection(DirectionTypes direction)
        {
            var approachSpeedsbyDirection = ApproachSpeedEvents
                .Where(a => a.Approach.DirectionType.Id == direction);
            var speedEvents = 0;
            if (approachSpeedsbyDirection.Any())
                speedEvents = Convert.ToInt32(Math.Round(approachSpeedsbyDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return speedEvents;
        }
    }
}