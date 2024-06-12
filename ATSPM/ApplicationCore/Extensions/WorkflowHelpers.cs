#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/WorkflowHelpers.cs
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
using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class WorkflowHelpers
    {
        public static IReadOnlyList<CorrectedDetectorEvent> FilterCorrectedDetectorEvents(this Tuple<Approach, IEnumerable<CorrectedDetectorEvent>> input)
        {
            return FilterCorrectedDetectorEvents(input.Item1, input.Item2);
        }

        public static IReadOnlyList<CorrectedDetectorEvent> FilterCorrectedDetectorEvents(Approach approach, IEnumerable<CorrectedDetectorEvent> events)
        {
            return events?
                .Where(w => w.LocationIdentifier == approach?.Location.LocationIdentifier && approach.Detectors
                .Select(s => s.DetectorChannel)
                .Contains(w.DetectorChannel))
                .ToList();
        }

        //public static IReadOnlyList<Volumes> FilterCorrectedDetectorEvents(this Tuple<Tuple<Approach, Volumes>, Tuple<Approach, Volumes>> input)
        //{
        //    var 



        //    return input.Item2?
        //        .Where(w => w.LocationIdentifier == input.Item1?.Location.LocationIdentifier && input.Item1.Detectors
        //        .Select(s => s.DetectorChannel)
        //        .Contains(w.DetectorChannel))
        //        .ToList();
        //}
    }
}
