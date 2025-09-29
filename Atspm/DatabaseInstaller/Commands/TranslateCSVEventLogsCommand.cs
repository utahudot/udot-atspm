using DatabaseInstaller.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;

namespace DatabaseInstaller.Commands
{
    public class TranslateCSVEventLogsCommand : Command, ICommandOption<CSVTransferCommandConfiguration>
    {
        public TranslateCSVEventLogsCommand()
            : base("translate-csv", "Translate logs from a CSV file")
        {
            AddOption(FilePathOption);
            AddOption(BatchOption);
            AddOption(DeviceOption);
            AddOption(LocationsOption);
        }

        public Option<string> FilePathOption { get; set; } =
            new("--file", "Path to the source CSV file") { IsRequired = true };

        public Option<int?> DeviceOption { get; set; } =
            new("--device", "Id of Device Type used to import events for just that device type") { IsRequired = false };

        public Option<int?> BatchOption { get; set; } =
            new("--batch", "Size of batches for importing event logs") { IsRequired = false };

        public Option<string> LocationsOption { get; set; } =
            new("--locations", "Comma separated list of location identifiers") { IsRequired = false };


        public ModelBinder<CSVTransferCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<CSVTransferCommandConfiguration>();

            binder.BindMemberFromValue(b => b.FilePath, FilePathOption);
            binder.BindMemberFromValue(b => b.Device, DeviceOption);
            binder.BindMemberFromValue(b => b.Batch, BatchOption);
            binder.BindMemberFromValue(b => b.LocationIdentifier, LocationsOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<CSVTransferCommandConfiguration>().BindCommandLine();
            services.AddHostedService<TranslateCSVEventLogsService>();
        }
    }

    public class CSVTransferCommandConfiguration
    {
        public string FilePath { get; set; }
        public string DataType { get; set; }
        public int? Device { get; set; }
        public int? Batch { get; set; }
        public string LocationIdentifier { get; set; }
    }

}
