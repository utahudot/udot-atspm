#region license
// Copyright 2025 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.ATSPM.EventLogUtility.Commands/TransferLogsConsoleCommand.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using System.Text.RegularExpressions;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;
using Utah.Udot.Atspm.MySqlDatabaseProvider;
using Utah.Udot.Atspm.OracleDatabaseProvider;
using Utah.Udot.Atspm.PostgreSQLDatabaseProvider;
using Utah.Udot.Atspm.SqlDatabaseProvider;
using Utah.Udot.Atspm.SqlLiteDatabaseProvider;

namespace Utah.Udot.ATSPM.EventLogUtility.Commands
{
    /// <summary>
    /// Command for transferring event logs between repositories.
    /// </summary>
    public class TransferLogsConsoleCommand : Command, ICommandOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransferLogsConsoleCommand"/> class.
        /// </summary>
        public TransferLogsConsoleCommand() : base("transfer", "Transfers event logs between repositories")
        {
            var values = typeof(EventLogModelBase).Assembly.GetTypes()
                .Where(w => w.IsSubclassOf(typeof(EventLogModelBase)))
                .Select(s => Regex.Replace(s.Name, @"(?<=[a-z])([A-Z])", @"_$1").ToLower())
                .Prepend("all")
                .ToArray();

            DataTypeArgument.FromAmong(values);

            Start.AddValidator(r =>
            {
                if (r.GetValueForOption(End) < r.GetValueForOption(Start))
                    r.ErrorMessage = "Start date must be before end date";
            });
            End.AddValidator(r =>
            {
                if (r.GetValueForOption(Start) > r.GetValueForOption(End))
                    r.ErrorMessage = "End date must be after start date";
            });

            IncludeLocationOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExcludeLocationOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use include option when also using exclude option";
            });
            ExcludeLocationOption.AddValidator(r =>
            {
                if (r.GetValueForOption(IncludeLocationOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use exclude option when also using include option";
            });

            SourceRepository.AddAlias("-sr");
            DestinationRepository.AddAlias("-dr");

            AddArgument(DataTypeArgument);
            AddGlobalOption(SourceRepository);
            AddGlobalOption(DestinationRepository);
            AddGlobalOption(IncludeLocationOption);
            AddGlobalOption(ExcludeLocationOption);
            AddGlobalOption(Start);
            AddGlobalOption(End);
            AddGlobalOption(DeviceIdIncludeCommandOption);
        }

        public Argument<string> DataTypeArgument { get; set; } = new Argument<string>("type", () => "all", "Event log data type to transfer");

        public Option<RepositoryConfiguration> SourceRepository { get; set; } = new("--source-repository", description: "Source repository configuration for transferring logs. Use <Provider>|<ConnectionString>",
            parseArgument: result =>
        {
            var value = result.Tokens.SingleOrDefault()?.Value;

            if (value is null || !value.Contains('|'))
            {
                result.ErrorMessage = "Invalid format. Use <Provider>|<ConnectionString>";
                return null;
            }

            var split = value.Split('|');

            return new RepositoryConfiguration
            {
                Provider = split[0],
                ConnectionString = split[1]
            };
        })
        {
            //IsRequired = true
        };

        public Option<RepositoryConfiguration> DestinationRepository { get; set; } = new("--destination-repository", description: "Destination repository configuration for transferring logs. Use <Provider>|<ConnectionString>",
            parseArgument: result =>
            {
                var value = result.Tokens.SingleOrDefault()?.Value;

                if (value is null || !value.Contains('|'))
                {
                    result.ErrorMessage = "Invalid format. Use <Provider>|<ConnectionString>";
                    return null;
                }

                var split = value.Split('|');

                return new RepositoryConfiguration
                {
                    Provider = split[0],
                    ConnectionString = split[1]
                };
            })
        {
            //IsRequired = true
        };

        public StartOption Start { get; set; } = new StartOption();

        public EndOption End { get; set; } = new EndOption();

        public LocationIncludeCommandOption IncludeLocationOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeLocationOption { get; set; } = new();

        public DeviceIncludeCommandOption DeviceIdIncludeCommandOption { get; set; } = new();

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<EventLogTransferOptions>(host.Configuration.GetSection(nameof(EventLogTransferOptions)));

            var binder = new ModelBinder<EventLogTransferOptions>();

            binder.BindMemberFromValue(b => b.SourceRepository, SourceRepository);
            binder.BindMemberFromValue(b => b.DestinationRepository, DestinationRepository);
            binder.BindMemberFromValue(b => b.StartDate, Start);
            binder.BindMemberFromValue(b => b.EndDate, End);
            binder.BindMemberFromValue(b => b.IncludedLocations, IncludeLocationOption);
            binder.BindMemberFromValue(b => b.ExcludedLocations, ExcludeLocationOption);
            binder.BindMemberFromValue(b => b.IncludedDeviceIds, DeviceIdIncludeCommandOption);
            binder.BindMemberFromValue(b => b.DataType, DataTypeArgument);

            services.AddOptions<EventLogTransferOptions>()
            .Configure<BindingContext>((a, b) =>
            {
                binder.UpdateInstance(a, b);
            });

            services.AddKeyedScoped<EventLogContext>(nameof(EventLogTransferOptions.SourceRepository), (s, o) =>
            {
                var builder = DbProvider(s.GetService<IOptions<EventLogTransferOptions>>().Value.SourceRepository)
                .EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment());
                return new EventLogContext(builder.Options);
            });

            services.AddKeyedScoped<EventLogContext>(nameof(EventLogTransferOptions.DestinationRepository), (s, o) =>
            {
                var builder = DbProvider(s.GetService<IOptions<EventLogTransferOptions>>().Value.DestinationRepository)
                .EnableSensitiveDataLogging(host.HostingEnvironment.IsDevelopment());
                return new EventLogContext(builder.Options);
            });

            services.AddHostedService<EventLogTransferHostedService>();
        }

        private DbContextOptionsBuilder<EventLogContext> DbProvider(RepositoryConfiguration config)
        {
            var builder = new DbContextOptionsBuilder<EventLogContext>();
            builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            switch (config.Provider)
            {
                case SqlServerProvider.ProviderName:
                    {
                        return builder.UseSqlServer(config.ConnectionString, opt => opt.MigrationsAssembly(SqlServerProvider.Migration));
                    }

                case PostgreSQLProvider.ProviderName:
                    {
                        return builder.UseNpgsql(config.ConnectionString, opt => opt.MigrationsAssembly(PostgreSQLProvider.Migration));
                    }

                case SqlLiteProvider.ProviderName:
                    {
                        return builder.UseSqlite(config.ConnectionString, opt => opt.MigrationsAssembly(SqlLiteProvider.Migration));
                    }

                case MySqlProvider.ProviderName:
                    {
                        return builder.UseMySql(ServerVersion.AutoDetect(config.ConnectionString), opt => opt.MigrationsAssembly(SqlLiteProvider.Migration));
                    }

                case OracleProvider.ProviderName:
                    {
                        return builder.UseOracle(config.ConnectionString, opt => opt.MigrationsAssembly(OracleProvider.Migration));
                    }

                default:
                    {
                        return builder.UseSqlServer(config.ConnectionString, opt => opt.MigrationsAssembly(SqlServerProvider.Migration));
                    }
            }
        }

        public class DeviceIncludeCommandOption : Option<IEnumerable<int>>
        {
            public DeviceIncludeCommandOption() : base("--include-device", "List of device id's to include")
            {
                AllowMultipleArgumentsPerToken = true;
                AddAlias("-id");
            }
        }
    }
}
