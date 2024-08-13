
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data;
using System.Data.SqlClient;
using System.IO.Compression;

public class ExtractOldArchiveCommand : Command
{
    private readonly IIndianaEventLogRepository indianaEventLogRepository;
    private readonly IServiceProvider serviceProvider;

    public ExtractOldArchiveCommand(IIndianaEventLogRepository indianaEventLogRepository, IServiceProvider serviceProvider) : base("extract-old-archive", "Extract logs command")
    {
        AddOption(new Option<string>("--signalid", "The signal ID"));
        AddOption(new Option<DateTime>("--timestamp", "The timestamp"));
        Handler = CommandHandler.Create<string, DateTime>(Execute);
        this.indianaEventLogRepository = indianaEventLogRepository;
        this.serviceProvider = serviceProvider;
    }

    private void Execute(string signalId, DateTime timeStamp)
    {
        // Your code from the Main method goes here
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetEventLogDBConnectionString");
        string targetConfigConnectionString = config.GetConnectionString("TargetConfigDBConnectionString");

        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"target is {targetConnectionString}");
        Console.WriteLine($"Press any key to continue otherwise close the window");
        Console.ReadKey();
        string selectQuery = $"SELECT LogData FROM [dbo].[ControllerLogArchives] where SignalId = {signalId} and ArchiveDate = '{timeStamp.Date}'";
        string deviceQuery = $"SELECT D.\"Id\", L.\"LocationIdentifier\" FROM public.\"Devices\" D Join public.\"Locations\" L on D.\"LocationId\" = L.\"Id\" Where L.\"LocationIdentifier\" = '{signalId}' AND L.\"VersionAction\" != 10 Order by L.\"Start\" desc Limit 1";
        int yourRecordID = 1;

        using (SqlConnection connection = new SqlConnection(sourceConnectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand(selectQuery, connection))
            {
                Console.WriteLine($"Reading data from table for {signalId} on {timeStamp}");
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"Data retrieved from table...");
                        byte[] compressedData = (byte[])reader["LogData"];

                        byte[] decompressedData;

                        Console.WriteLine($"Decompressing data...");
                        using (MemoryStream memoryStream = new MemoryStream(compressedData))
                        {
                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                            {
                                using (MemoryStream decompressedStream = new MemoryStream())
                                {
                                    gzipStream.CopyTo(decompressedStream);
                                    decompressedData = decompressedStream.ToArray();
                                }
                            }
                        }

                        Console.WriteLine($"Data decompressed...");
                        string json = System.Text.Encoding.UTF8.GetString(decompressedData);

                        Console.WriteLine($"Deserializing data...");

                        // Deserialize the JSON to an object
                        var jsonObject = JsonConvert.DeserializeObject<List<ArchiveLogs.Data.Models.ControllerEventLog>>(json);
                        Console.WriteLine($"Data deserialized...");

                        jsonObject.ForEach(x => x.SignalId = signalId);
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

                        var indianaEvents = ConvertToCompressedEvents(jsonObject, signalId, DateOnly.FromDateTime(timeStamp), deviceId);



                        using (var scope = serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetService<EventLogContext>();

                            if (context != null)
                            {
                                context.CompressedEvents.Add(indianaEvents);
                                context.SaveChanges();
                            }
                        }


                    }
                }
            }
        }

        // Add any additional code or logic as needed

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private CompressedEventLogs<IndianaEvent> ConvertToCompressedEvents(List<ArchiveLogs.Data.Models.ControllerEventLog> objectList, string locationIdentifier, DateOnly archiveDate, int deviceId)
    {
        DataTable dataTable = new DataTable();
        // Define the columns in the DataTable based on your object structure
        dataTable.Columns.Add("SignalID", typeof(string));
        dataTable.Columns.Add("TimeStamp", typeof(DateTime));
        dataTable.Columns.Add("EventCode", typeof(int));
        dataTable.Columns.Add("EventParam", typeof(int));
        // Add unique constraint on all columns
        UniqueConstraint uniqueConstraint = new UniqueConstraint(dataTable.Columns.Cast<DataColumn>().ToArray());
        dataTable.Constraints.Add(uniqueConstraint);
        var indianaEvents = new List<IndianaEvent>();
        foreach (var item in objectList.Distinct())
        {
            try
            {
                if (item.EventParam < 32000)
                    indianaEvents.Add(new IndianaEvent
                    {
                        LocationIdentifier = item.SignalId,
                        Timestamp = item.Timestamp,
                        EventCode = item.EventCode,
                        EventParam = (Int16)item.EventParam
                    });
            }
            catch
            {
                // Ignore errors on insert
            }
        }
        return new CompressedEventLogs<IndianaEvent>
        {
            LocationIdentifier = locationIdentifier,
            ArchiveDate = archiveDate,
            Data = indianaEvents,
            DeviceId = deviceId

        };
    }
}
