//#region license
//// Copyright 2024 Utah Departement of Transportation
//// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/GroupSignalByParameter.cs
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
//    public class BreakOutIndianaEvent : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>, Tuple<Location, IEnumerable<IndianaEvent>>>
//    {
//        public BreakOutIndianaEvent(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

//        protected override Task<Tuple<Location, IEnumerable<IndianaEvent>>> Process(Tuple<Location, IEnumerable<CompressedEventLogBase>> input, CancellationToken cancelToken = default)
//        {
//            var location = input.Item1;
//            var logs = input.Item2;

//            return Task.FromResult(Tuple.Create(location, logs));
//        }
//    }

//    public class BreakOutSpeedEvent : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>, Tuple<Location, IEnumerable<SpeedEvent>>>
//    {
//        public BreakOutSpeedEvent(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

//        protected override Task<Tuple<Location, IEnumerable<SpeedEvent>>> Process(Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>> input, CancellationToken cancelToken = default)
//        {
//            var location = input.Item1;
//            var logs = input.Item3;

//            return Task.FromResult(Tuple.Create(location, logs));
//        }
//    }

//    public class BreakoutCompressedEvents<T> : TransformProcessStepBase<Tuple<Location, IEnumerable<CompressedEventLogBase>>, Tuple<Location, IEnumerable<T>>> where T : EventLogModelBase
//    {
//        public BreakoutCompressedEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

//        protected override Task<Tuple<Location, IEnumerable<T>>> Process(Tuple<Location, IEnumerable<CompressedEventLogBase>> input, CancellationToken cancelToken = default)
//        {

//            return Task.FromResult(Tuple.Create(input.Item1, input.Item2.SelectMany(i => i.Data).Cast<T>()));
//            //throw new NotImplementedException();
//        }
//    }
//}
