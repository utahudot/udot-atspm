#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Utility/AuditPropertiesInterceptor.cs
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
using Microsoft.EntityFrameworkCore.Diagnostics;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.NetStandardToolkit.Authentication;
using Utah.Udot.NetStandardToolkit.Services;

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// Intercepts save changes to update audit properties
    /// </summary>
    public class AuditPropertiesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService<JwtUserSession> _currentUserService;

        /// <summary>
        /// Intercepts save changes to update audit properties
        /// </summary>
        /// <param name="currentUserService"></param>
        public AuditPropertiesInterceptor(ICurrentUserService<JwtUserSession> currentUserService) => _currentUserService = currentUserService;

        /// <inheritdoc/>
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            BeforeSaveTriggers(eventData.Context!);

            return base.SavingChanges(eventData, result);
        }

        /// <inheritdoc/>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = new())
        {
            BeforeSaveTriggers(eventData.Context!);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void BeforeSaveTriggers(DbContext context)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditProperties && (e.State == EntityState.Added || e.State == EntityState.Modified))
                .ToList();

            if (entries.Count == 0) return;

            var now = DateTimeOffset.UtcNow;
            var user = _currentUserService.GetCurrentUser();

            var name = !string.IsNullOrWhiteSpace(user?.FirstName) || !string.IsNullOrWhiteSpace(user?.LastName)
                ? $"{user.FirstName} {user.LastName}".Trim()
                : "System";

            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditProperties auditProperties)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditProperties.Created = now;
                        auditProperties.CreatedBy = name;
                    }

                    auditProperties.Modified = now;
                    auditProperties.ModifiedBy = name;
                }
            }
        }
    }

    //public class AuditPropertiesInterceptor : SaveChangesInterceptor
    //{
    //    private readonly ICurrentUserService<JwtUserSession> _currentUserService;

    //    public AuditPropertiesInterceptor(ICurrentUserService<JwtUserSession> currentUserService)
    //    {
    //        _currentUserService = currentUserService;
    //    }

    //    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    //    {
    //        UpdateAuditFields(eventData.Context);
    //        return base.SavingChanges(eventData, result);
    //    }

    //    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    //        DbContextEventData eventData,
    //        InterceptionResult<int> result,
    //        CancellationToken cancellationToken = default)
    //    {
    //        UpdateAuditFields(eventData.Context);
    //        return base.SavingChangesAsync(eventData, result, cancellationToken);
    //    }

    //    private void UpdateAuditFields(DbContext? context)
    //    {
    //        if (context == null) return;

    //        // 1. Filter directly by IAuditProperties interface
    //        var entries = context.ChangeTracker.Entries<IAuditProperties>()
    //            .Where(e => e.State is EntityState.Added or EntityState.Modified)
    //            .ToList(); // Materialize once after filtering

    //        if (entries.Count == 0) return;

    //        // 2. Deferred user resolution (only evaluated if auditing entities exist)
    //        var user = _currentUserService.GetCurrentUser();
    //        var name = !string.IsNullOrWhiteSpace(user?.FirstName) || !string.IsNullOrWhiteSpace(user?.LastName)
    //            ? $"{user.FirstName} {user.LastName}".Trim()
    //            : "System";

    //        var now = DateTimeOffset.UtcNow;

    //        foreach (var entry in entries)
    //        {
    //            if (entry.State == EntityState.Added)
    //            {
    //                // Preserve explicitly populated historic values if present
    //                if (entry.Entity.Created == default) entry.Entity.Created = now;
    //                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy)) entry.Entity.CreatedBy = name;
    //            }
    //            else if (entry.State == EntityState.Modified)
    //            {
    //                // 3. Guard against accidental SQL UPDATE overwrites on creation metadata
    //                entry.Property(x => x.Created).IsModified = false;
    //                entry.Property(x => x.CreatedBy).IsModified = false;
    //            }

    //            entry.Entity.Modified = now;
    //            entry.Entity.ModifiedBy = name;
    //        }
    //    }
    //}
}
