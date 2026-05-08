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
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Data
{
    /// <summary>
    /// The database context for the application's identity system, 
    /// extending <see cref="IdentityDbContext{TUser}"/> to include API key management.
    /// </summary>
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by this <see cref="DbContext"/>.</param>
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> for managing API keys.
        /// </summary>
        public DbSet<ApiKey> ApiKeys { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> for managing API key claims.
        /// </summary>
        public DbSet<ApiKeyClaim> ApiKeyClaims { get; set; }

        /// <summary>
        /// Configures the schema needed for the identity framework and the API key models.
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Entity<ApiKey>(entity =>
            {
                // Primary Key & Indexing
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.KeyHash).IsUnique();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .HasConversion(utcConverter)
                    .IsRequired();

                entity.Property(e => e.ExpiresAt)
                    .HasColumnType("timestamp with time zone")
                    .HasConversion(utcConverter);

                // Relationships
                entity.HasMany(e => e.Claims)
                      .WithOne()
                      .HasForeignKey(c => c.ApiKeyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.OwnerId).IsRequired();
            });

            builder.Entity<ApiKeyClaim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Value).IsRequired();
            });
        }
    }
}