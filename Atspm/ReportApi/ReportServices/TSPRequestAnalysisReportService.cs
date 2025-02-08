#region license
// Copyright 2024 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/PurdueCoordinationDiagramReportService.cs
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

using Utah.Udot.Atspm.Business.TSPRequestAnalysis;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Purdue coordination diagram report service
    /// </summary>
    public class TSPRequestAnalysisReportService : ReportServiceBase<TSPRequestAnalysisOptions, IEnumerable<TSPRequestAnalysisResult>>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly LocationPhaseService LocationPhaseService;
        private readonly ILocationRepository LocationRepository;
        private readonly CycleService cycleService;

        /// <inheritdoc/>
        public TSPRequestAnalysisReportService(
            IIndianaEventLogRepository controllerEventLogRepository,
            LocationPhaseService LocationPhaseService,
            ILocationRepository LocationRepository,
            CycleService cycleService)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationPhaseService = LocationPhaseService;
            this.LocationRepository = LocationRepository;
            this.cycleService = cycleService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TSPRequestAnalysisResult>> ExecuteAsync(TSPRequestAnalysisOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var locationIdentifiers = new List<string>
            {
                "6039",
                "6087",
                "6078",
                "6014",
                "6099",
                "6079",
                "6200",
                "6201",
                "6401",
                "6511",
                "6392",
                "6391",
                "6035",
                "6036",
                "6032",
                "6037",
                "6038",
                "6206",
                "6635",
                "6634",
                "6633",
                "6417",
                "6461",
                "6325",
                "6328",
                "6324",
                "6322",
                "6330",
                "6321",
                "6316",
                "6317",
                "6402",
                "6404",
                "6315",
                "6319",
                "6320",
                "6628",
                "6627",
                "6619",
                "6625",
                "6626",
                "6624",
                "6642",
                "6643",
                "6462",
                "6410",
                "6463",
                "6409",
                "6408",
                "6407",
                "6406",
                "6427",
                "6405",
                "6465",
                "6641",
                "6411",
                "6147",
                "6137",
                "6141",
                "6133",
                "6139",
                "6142",
                "6131",
                "6393",
                "6394",
                "6303",
                "6308",
                "6311",
                "6313",
                "6314",
                "6525",
                "6526",
                "6527",
                "6528",
                "6530",
                "6326",
                "6327",
                "6449",
                "6448",
                "6447",
                "6446",
                "6445",
                "6444",
                "6443",
                "6442",
                "6023",
                "6074",
                "6024",
                "6025",
                "6016",
                "6028",
                "6134",
                "6132",
                "6026",
                "6022",
                "6021",
                "6020",
                "6066",
                "6067",
                "6065",
                "6061",
                "6017",
                "6652",
                "6654",
                "6090",
                "6012",
                "6329",
                "6312",
                "6519",
                "6532",
                "6416",
                "6415",
                "6413",
                "6412",
                "6398",
                "6399",
                "6387",
                "6388",
                "6502",
                "6503",
                "6504",
                "6506",
                "6306",
                "6518",
                "6521",
                "6082",
                "6081",
                "6080",
                "6198",
                "6323",
                "6516",
                "6515",
                "6514",
                "6513",
                "6512",
                "6310",
                "6309",
                "6423",
                "6520",
                "6529",
                "6304",
                "6305",
                "6302",
                "6510",
                "6509",
                "6524",
                "6522"
            };
            var locations = LocationRepository.GetLatestVersionOfAllLocations(Convert.ToDateTime("10/8/24"))
                .Where(l => locationIdentifiers.Contains(l.LocationIdentifier));
            if (locations == null)
            {
                //return BadRequest("locations not found");
                return await Task.FromException<IEnumerable<TSPRequestAnalysisResult>>(new NullReferenceException("Location not found"));
            }
            var results = new List<TSPRequestAnalysisResult>();
            foreach (var location in locations)
            {
                var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(location.LocationIdentifier, parameter.Start, parameter.End).ToList();
                if (controllerEventLogs.IsNullOrEmpty())
                {
                    continue;
                }

                var targetEventCodes = controllerEventLogs.Where(c => c.EventCode == 113 || c.EventCode == 114).ToList();

                if (targetEventCodes.IsNullOrEmpty()) { continue; }
                var cycleEventCodes = new List<int>() { 1, 8, 9, 61, 63, 64 };
                var cycleEvents = controllerEventLogs.Where(c => c.EventParam == 2 && cycleEventCodes.Contains(c.EventCode)).ToList();
                if (cycleEvents.IsNullOrEmpty())
                {
                    cycleEvents = controllerEventLogs.Where(c => c.EventParam == 6 && cycleEventCodes.Contains(c.EventCode)).ToList();
                }
                if (cycleEvents.IsNullOrEmpty())
                {
                    controllerEventLogs = null;
                    continue;
                }
                controllerEventLogs = null;
                var cycles = cycleService.GetGreenToGreenCycles(parameter.Start, parameter.End, cycleEvents);
                var tasks = new List<Task<TSPRequestAnalysisResult>>();
                foreach (var cycle in cycles)
                {
                    if (targetEventCodes.Any(c => c.Timestamp >= cycle.StartTime && c.Timestamp <= cycle.EndTime && (c.EventCode == 113 || c.EventCode == 114)))
                    {
                        results.Add(new TSPRequestAnalysisResult(
                            location.LocationIdentifier,
                            cycle.StartTime,
                            cycle.EndTime,
                            targetEventCodes.Count(c => c.Timestamp >= cycle.StartTime && c.Timestamp <= cycle.EndTime && c.EventCode == 113),
                            targetEventCodes.Count(c => c.Timestamp >= cycle.StartTime && c.Timestamp <= cycle.EndTime && c.EventCode == 114)
                            ));
                    }
                }
            }

            return results;
        }
    }
    
}
