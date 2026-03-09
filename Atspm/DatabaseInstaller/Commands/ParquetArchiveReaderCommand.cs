#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/ArchiveParquetCommand.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
#endregion
using DatabaseInstaller.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;

namespace DatabaseInstaller.Commands
{
    public class ArchiveParquetCommand : Command, ICommandOption<ArchiveParquetCommandConfiguration>
    {
        public ArchiveParquetCommand() : base("parquet", "Read parquet files from disk and insert event logs into the database")
        {
            AddOption(FolderPrefixOption);
            AddOption(InputPathOption);
            AddOption(StartOption);
            AddOption(EndOption);
            AddOption(LocationsOption);
            AddOption(DeviceOption);
        }
        public Option<string> FolderPrefixOption { get; set; } = new("--folder-prefix", "Folder prefix used in archive directory names (e.g. 'date', 'archivedDate', 'timestamp')") { IsRequired = true };
        public Option<string> InputPathOption { get; set; } = new("--input-path", "Path to the directory containing parquet files");
        public Option<DateTime> StartOption { get; set; } = new("--start", "Start date filter");
        public Option<DateTime> EndOption { get; set; } = new("--end", "End date filter");
        public Option<string> LocationsOption { get; set; } = new("--locations", "Comma separated list of location identifiers") { IsRequired = false };
        public Option<int?> DeviceOption { get; set; } = new("--device", "Id of Device Type to filter by") { IsRequired = false };

        public ModelBinder<ArchiveParquetCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<ArchiveParquetCommandConfiguration>();
            binder.BindMemberFromValue(b => b.FolderPrefix, FolderPrefixOption);
            binder.BindMemberFromValue(b => b.InputPath, InputPathOption);
            binder.BindMemberFromValue(b => b.Start, StartOption);
            binder.BindMemberFromValue(b => b.End, EndOption);
            binder.BindMemberFromValue(b => b.Locations, LocationsOption);
            binder.BindMemberFromValue(b => b.Device, DeviceOption);
            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddOptions<ArchiveParquetCommandConfiguration>().BindCommandLine();
            services.AddSingleton(GetOptionsBinder());
            services.AddHostedService<ArchiveParquetHostedService>();
        }
    }
    public class ArchiveParquetCommandConfiguration
    {
        public string InputPath { get; set; }
        public DateTime? Start { get; set; }   // null = all dates
        public DateTime? End { get; set; }     // null = all dates
        public string Locations { get; set; }  // null = all locations
        public int? Device { get; set; }
        public string FolderPrefix { get; set; }
    }
}