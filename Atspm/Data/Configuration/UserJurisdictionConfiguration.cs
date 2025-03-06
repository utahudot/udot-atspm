#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/UserJurisdictionConfiguration.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Utah.Udot.Atspm.Data.Configuration
{
    /// <summary>
    /// User jurisdiction configuration
    /// </summary>
    public class UserJurisdictionConfiguration : IEntityTypeConfiguration<UserJurisdiction>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<UserJurisdiction> builder)
        {
            builder
            .HasKey(ur => new { ur.UserId, ur.JurisdictionId });

            //builder
            //    .HasOne(ur => ur.User)
            //    .WithMany(u => u.UserJurisdictions)
            //    .HasForeignKey(ur => ur.UserId);

            builder
                .HasOne(ur => ur.Jurisdiction)
                .WithMany(r => r.UserJurisdictions)
                .HasForeignKey(ur => ur.JurisdictionId);
        }
    }
}
