using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
                        var user = _currentUserService.GetCurrentUser().FullName ?? "System";

                        auditProperties.Created = now.Value;
                        auditProperties.Modified = now.Value;

                        auditProperties.CreatedBy = user;
                        auditProperties.ModifiedBy = user;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        auditProperties.Modified = DateTime.UtcNow;
                        auditProperties.ModifiedBy = _currentUserService.GetCurrentUser().FullName ?? "System";
                    }
                }
            }
        }
    }
}
