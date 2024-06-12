#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/ApproachResult.cs
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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace ATSPM.Application.Business.Common
{
    public class ApproachResult : LocationResult
    {
        public ApproachResult()
        {
                
        }

        public int ApproachId { get; set; }
        public string ApproachDescription { get; set; }

        public ApproachResult(int approachId, string locationId, DateTime start, DateTime end) : base(locationId, start, end)
        {
            ApproachId = approachId;
        }
    }
}
