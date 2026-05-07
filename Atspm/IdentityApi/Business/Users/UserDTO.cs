#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Identity.Business.Users/UserDTO.cs
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

namespace Identity.Business.Users
{
    public class UserAreaDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UserJurisdictionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UserRegionDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Agency { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public ICollection<string> Roles { get; set; } = [];
        public ICollection<UserJurisdictionDTO> Jurisdictions { get; set; } = [];
        public ICollection<UserAreaDTO> Areas { get; set; } = [];
        public ICollection<UserRegionDTO> Regions { get; set; } = [];
        public ICollection<int> JurisdictionIds { get; set; } = [];
        public ICollection<int> AreaIds { get; set; } = [];
        public ICollection<int> RegionIds { get; set; } = [];
    }
}
