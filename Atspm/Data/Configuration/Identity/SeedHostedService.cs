using Microsoft.Extensions.Hosting;

namespace Utah.Udot.Atspm.Data.Configuration.Identity
{
    public class SeedHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //await RolesAndClaimsDBInitializer.SeedRolesAndClaims(_serviceProvider);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

}
