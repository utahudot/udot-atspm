#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCoreTests - ApplicationCoreTests.Analysis.WorkflowFilterTests/FilterPriorityDataTests.cs
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
    public class FilterPriorityDataTests : WorkflowFilterTestsBase
    {
        public FilterPriorityDataTests(ITestOutputHelper output) : base(output)
        {
            filteredList.Add((int)112);
            filteredList.Add((int)113);
            filteredList.Add((int)114);

            sut = new FilterPriorityData();
        }
    }
}
