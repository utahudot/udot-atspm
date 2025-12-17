using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.ATSPM.EventLogUtility.Commands
{
    /// <summary>
    /// Defines CLI command process-parquet that accepts a --path, triggers the 
    /// ProcessParquetHostedService to run the parquet file ingestion workflow.
    /// </summary>
    public class ProcessParquetCommand : Command, ICommandOption<ProcessParquetConfiguration>
    {
        public ProcessParquetCommand() : base("process-parquet", "Import and decode event logs from Parquet files")
        {
            AddGlobalOption(PathCommandOption);
        }

        public PathCommandOption PathCommandOption { get; set; } = new();

        public ModelBinder<ProcessParquetConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<ProcessParquetConfiguration>();
            binder.BindMemberFromValue(b => b.Path, PathCommandOption);
            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<ProcessParquetConfiguration>().BindCommandLine();
            services.AddHostedService<ProcessParquetHostedService>();
        }
    }
}
