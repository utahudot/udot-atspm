#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity/IRoleService.cs
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
    /// Service contract for handling system roles and associated security permissions.
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// Registers a new security role in the system.
        /// </summary>
        /// <param name="roleName">The unique name of the role to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateRoleAsync(string roleName);

        /// <summary>
        /// Deletes a security role if it has no active user assignments.
        /// </summary>
        /// <param name="roleName">The unique name of the role to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteRoleAsync(string roleName);

        /// <summary>
        /// Retrieves all security roles registered in the system along with their active permissions.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the list of security roles.</returns>
        Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync();

        /// <summary>
        /// Retrieves the list of active functional permission claims assigned to a specific role.
        /// </summary>
        /// <param name="roleName">The unique name of the target role.</param>
        /// <returns>A task representing the asynchronous operation, containing the collection of permission claims.</returns>
        Task<IEnumerable<string>> GetClaimsForRoleAsync(string roleName);

        /// <summary>
        /// Synchronizes permission claims to a role atomically (adds new ones, removes deleted ones).
        /// </summary>
        /// <param name="roleName">The unique name of the target role.</param>
        /// <param name="permissions">The updated set of permission claims.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SyncClaimsToRoleAsync(string roleName, IEnumerable<string> permissions);

        /// <summary>
        /// Retrieves all possible functional permissions defined in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing the list of all permission values.</returns>
        Task<IEnumerable<string>> GetAllSystemPermissionsAsync();
    }
}
