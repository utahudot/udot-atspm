#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Specifications/LocationIdentifierSpecification.cs
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

using Utah.Udot.NetStandardToolkit.Specifications;

namespace Utah.Udot.Atspm.Specifications
{
    /// <summary>
    /// A specification that filters <see cref="Location"/> entities by their unique identifier.
    /// </summary>
    public class LocationIdentifierSpecification : BaseSpecification<Location>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationIdentifierSpecification"/> class
        /// with the provided location identifier.
        /// </summary>
        /// <param name="locationIdentifier">
        /// The unique identifier of the <see cref="Location"/> to filter for.
        /// </param>
        /// <remarks>
        /// This specification returns only the location that matches the given identifier.
        /// </remarks>
        public LocationIdentifierSpecification(string locationIdentifier) : base(s => s.LocationIdentifier == locationIdentifier)
        {
        }
    }

}
