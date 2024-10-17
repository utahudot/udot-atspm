using global::DatabaseInstaller.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Configuration.Identity;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.NetStandardToolkit.Extensions;


namespace DatabaseInstaller.Services
{
    public class MoveEventLogsSqlServerToPostgresHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<CopyConfigCommandConfiguration> _config;
        private readonly ILogger<UpdateCommandHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IndianaEventLogEFRepository _indianaEventLogEFRepository;

        public MoveEventLogsSqlServerToPostgresHostedService(
            IServiceProvider serviceProvider,
            IOptions<CopyConfigCommandConfiguration> config,
            ILogger<UpdateCommandHostedService> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IEventLogRepository eventLogRepository
            //IndianaEventLogEFRepository indianaEventLogEFRepository
            )
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
            _eventLogRepository = eventLogRepository;
            //_indianaEventLogEFRepository = indianaEventLogEFRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var serviceProvider = scope.ServiceProvider;

                // SQL Server DbContext to read logs
                var sqlOptions = new DbContextOptionsBuilder<EventLogContext>()
                    .UseSqlServer(_config.Value.Source)
                    .Options;

                using var sqlServerContext = new EventLogContext(sqlOptions);
                var sqlSeverRepository = new IndianaEventLogEFRepository(sqlServerContext, serviceProvider.GetService<ILogger<IndianaEventLogEFRepository>>());
                var sqltestSeverRepository = new EventLogEFRepository(sqlServerContext, serviceProvider.GetService<ILogger<EventLogEFRepository>>());


                // PostgreSQL DbContext to write logs
                //var postgresOptions = new DbContextOptionsBuilder<EventLogContext>()
                //    .UseNpgsql(_config.Value.Target)
                //    .Options;

                //using var postgresContext = new EventLogContext(postgresOptions);
                //var postgresSeverRepository = new IndianaEventLogEFRepository(postgresContext, serviceProvider.GetService<ILogger<IndianaEventLogEFRepository>>());

                var logs = sqltestSeverRepository.GetArchivedEvents("4613", DateOnly.FromDateTime(Convert.ToDateTime("2024-09-25")), DateOnly.FromDateTime(Convert.ToDateTime("2024-10-16")));

                _eventLogRepository.AddRange(logs);

                // Fetch logs from SQL Server
                //var logs = sqlSeverRepository.GetEventsBetweenDates("4613", Convert.ToDateTime("2024-09-24"), Convert.ToDateTime("2024-09-25"));
                //List<CompressedEventLogs<IndianaEvent>> archiveLogs = new List<CompressedEventLogs<IndianaEvent>>();
                //archiveLogs.Add(new CompressedEventLogs<IndianaEvent>
                //{
                //     ArchiveDate= DateOnly.FromDateTime(Convert.ToDateTime("2024-09-24")),
                //      DataType
                //});
                //_indianaEventLogEFRepository.AddRange(logs);


                _logger.LogInformation("Event logs moved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while moving event logs: {Exception}", ex);
            }
            finally
            {
                _logger.LogInformation("Shutting down the application after moving logs.");
                _hostApplicationLifetime.StopApplication();
            }
        }



        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }


}
