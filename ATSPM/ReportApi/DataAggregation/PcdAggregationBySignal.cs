#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - MOE.Common.Business.DataAggregation/PcdAggregationBySignal.cs
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
    public class PcdAggregationBySignal : AggregationBySignal
    {
        private readonly IApproachPcdAggregationRepository approachPcdAggregationRepository;

        public PcdAggregationBySignal(
            ApproachPcdAggregationOptions approachPcdAggregationOptions,
            Location signal,
            AggregationOptions options
            ) : base(
            approachPcdAggregationOptions, signal, options)
        {
            ApproachPcds = new List<PcdAggregationByApproach>();
            GetApproachPcdAggregationContainersForAllApporaches(approachPcdAggregationOptions, signal, options);
            LoadBins(null, null, options);
        }

        public PcdAggregationBySignal(
            ApproachPcdAggregationOptions approachPcdAggregationOptions,
            Location signal,
            int phaseNumber,
            IApproachPcdAggregationRepository approachPcdAggregationRepository,
            AggregationOptions options
            ) : base(approachPcdAggregationOptions, signal, options)
        {
            ApproachPcds = new List<PcdAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachPcds.Add(
                        new PcdAggregationByApproach(
                            approach,
                            approachPcdAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachPcdAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachPcds.Add(
                            new PcdAggregationByApproach(
                                approach,
                                approachPcdAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachPcdAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
            this.approachPcdAggregationRepository = approachPcdAggregationRepository;
        }

        public PcdAggregationBySignal(
            ApproachPcdAggregationOptions approachPcdAggregationOptions,
            Location signal,
            DirectionTypes direction,
            AggregationOptions options
            ) : base(approachPcdAggregationOptions, signal, options)
        {
            ApproachPcds = new List<PcdAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.Id == direction)
                {
                    ApproachPcds.Add(
                        new PcdAggregationByApproach(
                            approach,
                            approachPcdAggregationOptions,
                            options.Start,
                            options.End,
                            true,
                            options.DataType,
                            approachPcdAggregationRepository,
                            options
                            ));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachPcds.Add(
                            new PcdAggregationByApproach(
                                approach,
                                approachPcdAggregationOptions,
                                options.Start,
                                options.End,
                                false,
                                options.DataType,
                                approachPcdAggregationRepository,
                                options
                                ));
                }
            LoadBins(null, null, options);
        }

        public List<PcdAggregationByApproach> ApproachPcds { get; }

        protected override void LoadBins(SignalAggregationMetricOptions signalAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachPcdAggregationContainer in ApproachPcds)
                        bin.Sum += approachPcdAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachPcds.Count > 0 ? bin.Sum / ApproachPcds.Count : 0;
                }
            }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions approachAggregationMetricOptions, Location signal, AggregationOptions options)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            {
                for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
                {
                    var bin = BinsContainers[i].Bins[binIndex];
                    foreach (var approachPcdAggregationContainer in ApproachPcds)
                        bin.Sum += approachPcdAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachPcds.Count > 0 ? bin.Sum / ApproachPcds.Count : 0;
                }
            }
        }


        private void GetApproachPcdAggregationContainersForAllApporaches(
            ApproachPcdAggregationOptions approachPcdAggregationOptions, Location signal, AggregationOptions options)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachPcds.Add(
                    new PcdAggregationByApproach(
                        approach,
                        approachPcdAggregationOptions,
                        options.Start,
                        options.End,
                        true,
                        options.DataType,
                        approachPcdAggregationRepository,
                        options
                        ));
                if (approach.PermissivePhaseNumber != null)
                    ApproachPcds.Add(
                        new PcdAggregationByApproach(
                            approach,
                            approachPcdAggregationOptions,
                            options.Start,
                            options.End,
                            false,
                            options.DataType,
                            approachPcdAggregationRepository,
                            options
                            ));
            }
        }


        public double GetPcdsByDirection(DirectionTypes direction)
        {
            double splitFails = 0;
            if (ApproachPcds != null)
                splitFails = ApproachPcds
                    .Where(a => a.Approach.DirectionType.Id == direction)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAveragePcdsByDirection(DirectionTypes direction)
        {
            var approachPcduresByDirection = ApproachPcds
                .Where(a => a.Approach.DirectionType.Id == direction);
            var splitFails = 0;
            if (approachPcduresByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachPcduresByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}