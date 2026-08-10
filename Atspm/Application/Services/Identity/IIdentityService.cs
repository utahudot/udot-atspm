#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity/IIdentityService.cs
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

using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Services.Identity
{
    /// <summary>
    /// Service contract for administrative user account operations and custom geographic profile linkages.
    /// </summary>
    public interface IIdentityService
    {
        /// <summary>
        /// Registers a new user in the system with associated roles, areas, regions, and jurisdictions.
        /// </summary>
        /// <param name="request">The details of the user to create.</param>
        /// <returns>A task representing the asynchronous operation, containing the created user details.</returns>
        Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);

        /// <summary>
        /// Retrieves detailed information about a user profile by ID, including roles and custom geographic boundaries.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, containing the user details.</returns>
        Task<UserResponseDto> GetUserByIdAsync(string id);

        /// <summary>
        /// Retrieves all registered users in the system, optimized to eager-load roles and custom geography without N+1 queries.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of all registered users.</returns>
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

        /// <summary>
        /// Updates a user account's profile details and synchronizes geographic bindings.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update.</param>
        /// <param name="request">The updated user details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateUserAsync(string id, UpdateUserRequestDto request);

        /// <summary>
        /// Deletes a user profile and cleans up all dependent database geographic mappings.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteUserAsync(string id);
    }
}
