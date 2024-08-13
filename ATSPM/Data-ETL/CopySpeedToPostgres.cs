using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;

public class CopySpeedToPostgres : Command
{
    private readonly ILogger<CopyConfigurationNewPostgres> logger;
    private readonly ISpeedEventLogRepository speedEventLogRepository;
    private readonly IServiceProvider serviceProvider;

    public CopySpeedToPostgres(ILogger<CopyConfigurationNewPostgres> logger, ISpeedEventLogRepository speedEventLogRepository, IServiceProvider serviceProvider) : base("copy-speed-postgres", "Copy speed Data for new data format in a posgres database")
    {

        var dateOption = new Option<DateTime?>("--date", "The date");
        var signaIdlOption = new Option<string?>("--signalid", "The signal id");
        AddOption(signaIdlOption);
        AddOption(dateOption);
        Handler = CommandHandler.Create<string, DateTime>(Execute);
        this.logger = logger;
        this.speedEventLogRepository = speedEventLogRepository;
        this.serviceProvider = serviceProvider;
    }

    private void Execute(string signalId, DateTime date)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();


        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetEventLogDBConnectionString");
        string targetConfigConnectionString = config.GetConnectionString("TargetConfigDBConnectionString");

        string deviceQuery = $"SELECT D.\"Id\", L.\"LocationIdentifier\" FROM public.\"Devices\" D Join public.\"Locations\" L on D.\"LocationId\" = L.\"Id\" Where L.\"LocationIdentifier\" = '{signalId}' AND L.\"VersionAction\" != 10 And D.\"DeviceType\" = 'Wavetronix' Order by L.\"Start\" desc Limit 1";
        var deviceId = -1;
        using (NpgsqlConnection targetConfigConnection = new NpgsqlConnection(targetConfigConnectionString))
        {
            try
            {
                targetConfigConnection.Open();

                using (NpgsqlCommand deviceCommand = new NpgsqlCommand(deviceQuery, targetConfigConnection))
                using (NpgsqlDataReader deviceReader = deviceCommand.ExecuteReader())
                {
                    while (deviceReader.Read())
                    {
                        deviceId = deviceReader.GetInt32(0); // Assuming the first column (Id) is an int                                       
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");

        using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
        {
            sourceConnection.Open();
            var queries = new Dictionary<string, string>();

            queries.Add("Speed_Events", $"Select DISTINCT [DetectorID]\r\n      ,[MPH]\r\n      ,[KPH]\r\n      ,[timestamp]\r\n  FROM [MOE].[dbo].[Speed_Events] Where timestamp between '{date}' and '{date.AddDays(1)}' and DetectorID like '{signalId}%'  ");






            // Define column mappings for each table
            var columnMappings = new Dictionary<string, Dictionary<string, string>>
    {
        {
            "Speed_Events", new Dictionary<string, string>
            {
                { "DetectorID", "DetectorId" },
                { "MPH", "Mph" },
                { "KPH", "Kph" },
                { "timestamp", "TimeStamp" }
            }
                },
    };

            var tableNameMappings = new Dictionary<string, string>
    {
        {"Speed_Events", "SpeedEvents" },
    };


            try
            {
                using (SqlCommand sourceCommand = new SqlCommand(queries.First().Value, sourceConnection))
                {
                    sourceCommand.CommandTimeout = 240;
                    using (SqlDataReader reader = sourceCommand.ExecuteReader())
                    {
                        ExportToPostgres(signalId, deviceId, DateOnly.FromDateTime(date), reader);
                    }
                }
                Console.WriteLine($"Data copied and bulk inserted for {queries.First().Key}.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, "Error copying data from SQL Server to Postgres");
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine("Data copied and bulk inserted.");
    }


    private void ExportToPostgres(string locationIdentifier, int deviceId, DateOnly dateToRetrieve, SqlDataReader reader)
    {
        var speeds = new List<SpeedEvent>();
        while (reader.Read())
        {
            var speed = new SpeedEvent
            {
                DetectorId = reader["DetectorID"].ToString(),
                Kph = (int)reader["KPH"],
                Mph = (int)reader["MPH"],
                Timestamp = (DateTime)reader["timestamp"]
            };
            speeds.Add(speed);
        }

        if (speeds.Count > 0)
        {
            CompressedEventLogs<SpeedEvent> archiveLog = new CompressedEventLogs<SpeedEvent>()
            {
                LocationIdentifier = locationIdentifier,
                DeviceId = deviceId,
                ArchiveDate = dateToRetrieve,
                Data = speeds
            };
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<EventLogContext>();

                if (context != null)
                {
                    context.CompressedEvents.Add(archiveLog);
                    context.SaveChanges();
                }
            }
        }
    }
}
