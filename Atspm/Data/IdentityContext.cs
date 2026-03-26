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

namespace Utah.Udot.Atspm.Data
{
    /// <summary>
    /// Identity database context
    /// </summary>
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Identity database context
        /// </summary>
        /// <param name="options"></param>
        public IdentityContext(DbContextOptions<IdentityContext> options)
            : base(options)
        {

        }

        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<ApiKeyClaim> ApiKeyClaims { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            Npgsql.NpgsqlConnectionStringBuilder


            base.OnModelCreating(builder);

            // Configuration for ApiKey
            builder.Entity<ApiKey>(entity =>
            {
                // Index the hash for high-performance lookups in the Auth Handler
                entity.HasIndex(e => e.KeyHash).IsUnique();

                // Relationship: One Key has many Claims
                entity.HasMany(e => e.Claims)
                      .WithOne()
                      .HasForeignKey(c => c.ApiKeyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ApiKeyClaim>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }

    public class ApiKey
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }

        // This links to the child table below
        public List<ApiKeyClaim> Claims { get; set; } = new List<ApiKeyClaim>();
    }

    public class ApiKeyClaim
    {
        public int Id { get; set; }
        public int ApiKeyId { get; set; } // Foreign Key
        public string Type { get; set; } = string.Empty;  // e.g., "role"
        public string Value { get; set; } = string.Empty; // e.g., "Admin"
    }
}