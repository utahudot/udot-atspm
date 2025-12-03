#region license
// Copyright 2025 Utah Departement of Transportation
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
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is IAuditProperties auditProperties)
                {
                    if (entry.State == EntityState.Added)
                    {
                        var now = new Lazy<DateTime>(() => DateTime.UtcNow);
                        var user = _currentUserService.GetCurrentUser();

                        var name = ($"{user.FirstName} {user.LastName}") ?? "System";

                        auditProperties.Created = now.Value;
                        auditProperties.Modified = now.Value;

                        auditProperties.CreatedBy = name;
                        auditProperties.ModifiedBy = name;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditProperties.Modified = DateTime.UtcNow;

                        var user = _currentUserService.GetCurrentUser();

                        auditProperties.ModifiedBy = ($"{user.FirstName} {user.LastName}") ?? "System";
                    }
                }
            }
        }
    }
}
