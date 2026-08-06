#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity.Dto/IdentityDtos.cs
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

using System.Collections.Generic;

namespace Utah.Udot.Atspm.Services.Identity.Dto
{
    /// <summary>
    /// Data transfer object used for creating a new user account administratively.
    /// </summary>
    public record CreateUserRequestDto(
        string Email,
        string FirstName,
        string LastName,
        string Agency,
        IEnumerable<string> Roles,
        IEnumerable<int> AreaIds,
        IEnumerable<int> RegionIds,
        IEnumerable<int> JurisdictionIds
    );

    /// <summary>
    /// Data transfer object used for modifying an existing user account.
    /// </summary>
    public record UpdateUserRequestDto(
        string Email,
        string FirstName,
        string LastName,
        string Agency,
        IEnumerable<string> Roles,
        IEnumerable<int> AreaIds,
        IEnumerable<int> RegionIds,
        IEnumerable<int> JurisdictionIds
    );

    /// <summary>
    /// Data transfer object representing a detailed user account view.
    /// Includes roles and assigned geographic profile links.
    /// </summary>
    public record UserResponseDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string Agency,
        IEnumerable<string> Roles,
        IEnumerable<int> AreaIds,
        IEnumerable<int> RegionIds,
        IEnumerable<int> JurisdictionIds
    );

    /// <summary>
    /// Data transfer object representing a security role in the system.
    /// </summary>
    public record RoleResponseDto(
        string RoleName,
        IEnumerable<string> AssignedPermissions
    );
}
