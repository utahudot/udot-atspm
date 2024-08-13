﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Specifications/LocationSpecifications.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    public class ActiveLocationSpecification : BaseSpecification<Location>
    {
        public ActiveLocationSpecification() : base(s => s.VersionAction != LocationVersionActions.Delete)
        {
            ApplyOrderByDescending(o => o.Start);
        }
    }

    public class LocationIdSpecification : BaseSpecification<Location>
    {
        public LocationIdSpecification(string locationId) : base(s => s.LocationIdentifier == locationId) { }
    }
}
