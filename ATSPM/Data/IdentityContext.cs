#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - %Namespace%/IdentityContext.cs
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class IdentityContext : IdentityDbContext<ApplicationUser>
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options)
    {

    }

}

//TODO: this should be moved into models
public class ApplicationUser : IdentityUser
{
    // Additional properties for your custom user entity
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Agency { get; set; }
    public string FullName { get { return $"{FirstName} {LastName}"; } }

    // Navigation property for user roles
    public ICollection<IdentityUserRole<string>> Roles { get; set; }
}