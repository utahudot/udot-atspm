#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Repositories.ConfigurationRepositories/IUserGeographyRepository.cs
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

namespace Utah.Udot.Atspm.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Composite repository for managing user geographic profiles (Areas, Regions, Jurisdictions)
    /// </summary>
    public interface IUserGeographyRepository
    {
        /// <summary>
        /// Retrieves user-to-area mappings for a list of user IDs, including Area navigation properties
        /// </summary>
        Task<IEnumerable<UserArea>> GetAreasByUserIdsAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Retrieves user-to-region mappings for a list of user IDs, including Region navigation properties
        /// </summary>
        Task<IEnumerable<UserRegion>> GetRegionsByUserIdsAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Retrieves user-to-jurisdiction mappings for a list of user IDs, including Jurisdiction navigation properties
        /// </summary>
        Task<IEnumerable<UserJurisdiction>> GetJurisdictionsByUserIdsAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Updates all user geographic profile mappings atomically in a single operation
        /// </summary>
        Task UpdateUserGeographyAsync(string userId, IEnumerable<int> areaIds, IEnumerable<int> regionIds, IEnumerable<int> jurisdictionIds);
    }
}
