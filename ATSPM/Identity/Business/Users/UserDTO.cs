#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Business.Users/UserDTO.cs
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
    public class UserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Agency { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        public ICollection<string> Roles { get; set; }
    }
}
