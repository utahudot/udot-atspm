//#region license
//// Copyright 2024 Utah Departement of Transportation
//// for ApplicationCore - ATSPM.Application.Analysis.WorkflowFilters/FilterEventCodeSignalBase.cs
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
//// http://www.apache.org/licenses/LICENSE-2.
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
//#endregion

//using System.Threading.Tasks.Dataflow;
//using Utah.Udot.Atspm.Data.Models.EventLogModels;

//namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
//{
//    public class GroupDetectorIndianaSpeedData : TransformManyProcessStepBase<IEnumerable<Compress>, Tuple<Detector, string, IEnumerable<IndianaEvent>>>
//    {
//        public GroupDetectorIndianaSpeedData(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

//        protected override Task<IEnumerable<Tuple<Detector, string, IEnumerable<IndianaEvent>>>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
//        {
//            var location = input.Item1;
//            var indianaLogs = input.Item2;

//            var result = new List<Tuple<Detector, string, IEnumerable<IndianaEvent>>>();
//            var locationIdentifier = location.LocationIdentifier;

//            foreach (var approach in location.Approaches)
//            {
//                foreach (var detector in approach.Detectors)
//                {
//                    var speedEvents = indianaLogs
//                        .Where(log => log.LocationIdentifier.Equals(locationIdentifier) && log.EventCode == 82 && log.EventParam == detector.DetectorChannel);

//                    result.Add(Tuple.Create(detector, detector.DectectorIdentifier, speedEvents));
//                }
//            }

//            return Task.FromResult<IEnumerable<Tuple<Detector, string, IEnumerable<IndianaEvent>>>>(result);
//        }
//    }
//}
