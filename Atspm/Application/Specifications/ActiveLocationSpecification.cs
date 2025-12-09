#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/ActiveLocationSpecification.cs
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
    /// <summary>
    /// A specification that filters for active <see cref="Location"/> entities.
    /// </summary>
    /// <remarks>
    /// This specification excludes locations that have a <see cref="LocationVersionActions.Delete"/> action
    /// and orders the results by the <see cref="Location.Start"/> property in descending order.
    /// </remarks>
    public class ActiveLocationSpecification : BaseSpecification<Location>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveLocationSpecification"/> class.
        /// </summary>
        /// <remarks>
        /// The base specification is configured to include only locations that are not marked for deletion,
        /// and results are ordered by the start date descending.
        /// </remarks>
        public ActiveLocationSpecification() : base(s => s.VersionAction != LocationVersionActions.Delete)
        {
            ApplyOrderByDescending(o => o.Start);
        }
    }
}
