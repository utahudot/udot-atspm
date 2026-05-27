#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data/IdentityContext.cs
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

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Data.Configuration.IdentityConfiguration;
using Utah.Udot.Atspm.Data.Models.IdentityModels;
using Utah.Udot.Atspm.Data.Utility;

namespace Utah.Udot.Atspm.Data
{
    /// <summary>
    /// The database context for the application's identity system, 
    /// extending <see cref="IdentityDbContext{TUser}"/> to include API key management.
    /// </summary>
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        /// <inheritdoc/>
        public IdentityContext() { }

        /// <inheritdoc/>
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> for managing API keys.
        /// </summary>
        public DbSet<ApiKey> ApiKeys { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> for managing API key claims.
        /// </summary>
        public DbSet<ApiKeyClaim> ApiKeyClaims { get; set; }

        /// <inheritdoc/>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ApplyDateTimeOffsetConverters();
            configurationBuilder.ApplyProviderDateTimeTypes(Database.ProviderName);
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ApiKeyConfiguration());
            modelBuilder.ApplyConfiguration(new ApiKeyClaimConfiguration());
            //TODO: Add ApplicationUser configuration

            base.OnModelCreating(modelBuilder);
        }
    }
}