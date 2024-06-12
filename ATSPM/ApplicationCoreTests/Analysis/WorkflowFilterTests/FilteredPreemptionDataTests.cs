#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.WorkflowFilterTests/FilteredPreemptionDataTests.cs
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
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Data.Enums;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowFilterTests
{
    public class FilteredPreemptionDataTests : WorkflowFilterTestsBase
    {
        public FilteredPreemptionDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)IndianaEnumerations.PreemptCallInputOn);
            filteredList.Add((int)IndianaEnumerations.PreemptGateDownInputReceived);
            filteredList.Add((int)IndianaEnumerations.PreemptCallInputOff);
            filteredList.Add((int)IndianaEnumerations.PreemptEntryStarted);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginTrackClearance);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginDwellService);
            filteredList.Add((int)110);
            filteredList.Add((int)IndianaEnumerations.PreemptionBeginExitInterval);

            sut = new FilteredPreemptionData();
        }
    }
}
