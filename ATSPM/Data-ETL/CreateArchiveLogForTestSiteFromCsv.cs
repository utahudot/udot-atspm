
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

public class CreateArchiveLogForTestSiteCsv : Command
{
    private readonly ILogger<CreateArchiveLogForTestSite> logger;
    private readonly IIndianaEventLogRepository indianaEventLogRepository;
    private readonly IServiceProvider serviceProvider;
    private readonly ILocationRepository locationRepository;

    public CreateArchiveLogForTestSiteCsv(
        ILogger<CreateArchiveLogForTestSite> logger,
        IIndianaEventLogRepository indianaEventLogRepository,
        IServiceProvider serviceProvider,
        ILocationRepository locationRepository) : base("create-test-csv", "Create Archive Log command")
    {
        var dateOption = new Option<DateTime?>("--date", "The date");
        dateOption.IsRequired = false; // Make the option not required
        AddOption(dateOption);

        Handler = CommandHandler.Create<DateTime?>(ExecuteAsync);
        this.logger = logger;
        this.indianaEventLogRepository = indianaEventLogRepository;
        this.serviceProvider = serviceProvider;
        this.locationRepository = locationRepository;
    }

    private async Task ExecuteAsync(DateTime? date)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetEventLogDBConnectionString");
        string targetConfigConnectionString = config.GetConnectionString("TargetConfigDBConnectionString");
        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");
        //Console.WriteLine($"Press any key to continue or close the window");
        //Console.ReadKey();

        string signalsFilePath = @"C:\Users\dlowe\Documents\PortlandData\Signals.csv";
        string eventsFilePath = @"C:\Users\dlowe\Documents\PortlandData\ATSPM Events_2023-05-15_2023-05-31.csv";

        var events = new List<IndianaEvent>();
        using (TextFieldParser parser = new TextFieldParser(eventsFilePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                events.Add(new IndianaEvent { LocationIdentifier = $"p-{fields[0]}", EventCode = short.Parse(fields[2]), EventParam = short.Parse(fields[3]), Timestamp = Convert.ToDateTime(fields[1]) });
            }
        }

        var signalIds = new List<string>();
        using (TextFieldParser parser = new TextFieldParser(signalsFilePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                signalIds.Add($"p-{fields[0]}");
            }
            signalIds = signalIds.Distinct().ToList();
            var locations = new List<Location>();
            foreach (var signalId in signalIds)
            {
                var location = locationRepository.GetList()
                   .Include(i => i.Devices)
               .FromSpecification(new LocationIdSpecification(signalId))
               .FromSpecification(new ActiveLocationSpecification())
               .OrderByDescending(l => l.Start)
               .FirstOrDefault();
                if (location != null)
                {
                    locations.Add(location);
                }
            }
            //var signalsQuery = "SELECT distinct [SignalID] FROM [MOE].[dbo].[Signals] where VersionActionId != 3 order by SignalID";
            //var devicesQuery = "SELECT D.\"Id\", L.\"LocationIdentifier\"\r\n\tFROM public.\"Devices\" D\r\n\tJoin public.\"Locations\" L on D.\"LocationId\" = L.\"Id\"";
            //var dateToRetrieve = date == null ? DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) : DateOnly.FromDateTime(date.Value);

            var endDate = Convert.ToDateTime("6/1/2023");

            for (DateTime dateToRetrieve = Convert.ToDateTime("5/15/2023"); dateToRetrieve < endDate; dateToRetrieve = dateToRetrieve.AddDays(1))
            {
                var archiveLogs = new List<CompressedEventLogs<IndianaEvent>>();
                foreach (var location in locations)
                {
                    try
                    {
                        CompressedEventLogs<IndianaEvent> archiveLog = new CompressedEventLogs<IndianaEvent>()
                        {
                            LocationIdentifier = location.LocationIdentifier,
                            DeviceId = location.Devices.FirstOrDefault().Id,
                            ArchiveDate = DateOnly.FromDateTime(dateToRetrieve),
                            Data = events.Where(e => e.LocationIdentifier == location.LocationIdentifier && e.Timestamp.Date == dateToRetrieve).ToList()
                        };
                        indianaEventLogRepository.Add(archiveLog);
                        Thread.Sleep(5000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    //archiveLogs.Add(archiveLog);
                }
                //foreach (var archiveLog in archiveLogs)
                //{
                //    using (var scope = serviceProvider.CreateScope())
                //    {
                //        var context = scope.ServiceProvider.GetService<EventLogContext>();

                //        if (context != null)
                //        {
                //            context.Add(archiveLog);
                //        }
                //    }
                //}
            }

            ////var locationDevicesDict = new Dictionary<string, int>();

            ////using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConfigConnectionString))
            ////{
            ////    try
            ////    {
            ////        targetConnection.Open();

            ////        using (NpgsqlCommand command = new NpgsqlCommand(devicesQuery, targetConnection))
            ////        using (NpgsqlDataReader reader = command.ExecuteReader())
            ////        {
            ////            while (reader.Read())
            ////            {
            ////                int deviceId = reader.GetInt32(0); // Assuming the first column (Id) is an int
            ////                string locationIdentifier = reader.GetString(1); // Assuming the second column (LocationIdentifier) is a string

            ////                // Add to dictionary
            ////                // Note: This assumes that each LocationIdentifier is unique. If not, you might need a List<int> as the dictionary value type
            ////                locationDevicesDict[locationIdentifier] = deviceId;
            ////            }
            ////        }
            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        Console.WriteLine($"An error occurred: {ex.Message}");
            ////    }
            ////}



            //try
            //{
            //    using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
            //    {
            //        //sourceConnection.Open();
            //        //var signals = new List<string>();
            //        //using (SqlCommand selectCommand = new SqlCommand(signalsQuery, sourceConnection))
            //        //{
            //        //    selectCommand.CommandTimeout = 120;
            //        //    Console.WriteLine($"Executing query: {signalsQuery}");
            //        //    using (SqlDataReader reader = selectCommand.ExecuteReader())
            //        //    {
            //        //        while (reader.Read())
            //        //        {
            //        //            signals.Add(reader["SignalID"].ToString());
            //        //        }
            //        //    }
            //        //}
            //        var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            //        //Parallel.ForEach(signals, options, signalId =>
            //        var archiveLogs = new List<CompressedEventLogs<IndianaEvent>>();
            //        foreach (var signalId in signals)
            //        {
            //            if (!locationDevicesDict.ContainsKey(signalId))
            //            {
            //                Console.WriteLine($"No device found for signal {signalId}");
            //                continue;
            //            }
            //            string selectQuery = $"SELECT LocationIdentifier, Timestamp, EventCode, EventParam FROM [dbo].[Controller_Event_Log] WHERE LocationIdentifier = '{signalId}' AND Timestamp between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";



            //            using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
            //            {
            //                List<IndianaEvent> eventLogs = new List<IndianaEvent>();
            //                selectCommand.CommandTimeout = 120;
            //                Console.WriteLine($"Executing query: {selectQuery}");
            //                using (SqlDataReader reader = selectCommand.ExecuteReader())
            //                {
            //                    while (reader.Read())
            //                    {
            //                        try
            //                        {
            //                            IndianaEvent eventLog = new IndianaEvent()
            //                            {
            //                                //LocationIdentifier = signalId,
            //                                Timestamp = (DateTime)reader["Timestamp"],
            //                                EventCode = (short)reader["EventCode"],
            //                                EventParam = Convert.ToInt16((int)reader["EventParam"])
            //                            };

            //                            eventLogs.Add(eventLog);
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            Console.WriteLine($" Event: {signalId}-{reader["Timestamp"]} EventCode:{reader["EventCode"]} EventParam:{reader["EventParam"]} Error reading record: {ex.Message}");
            //                        }
            //                    }
            //                }

            //                Console.WriteLine($"Controller Event Logs retrieved");
            //                if (eventLogs.Count > 0)
            //                {
            //                    CompressedEventLogs<IndianaEvent> archiveLog = new CompressedEventLogs<IndianaEvent>()
            //                    {
            //                        LocationIdentifier = signalId,
            //                        DeviceId = locationDevicesDict[signalId],
            //                        ArchiveDate = dateToRetrieve,
            //                        Table = eventLogs
            //                    };
            //                    archiveLogs.Add(archiveLog);
            //                    //archiveLogs.Add(archiveLog);

            //                    //byte[] compressedData = GZipCompressToByte(JsonSerializer.Serialize(archiveLog.Table.Select(c => new { c.LocationIdentifier, c.EventCode, c.EventParam, c.Timestamp }), new JsonSerializerOptions()));
            //                    //byte[] compressedData = SerializeAndCompress<List<IndianaEvent>>(eventLogs);

            //                    //using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
            //                    //{
            //                    //    targetConnection.Open();

            //                    //    string insertQuery = "INSERT INTO public.\"CompressedEvents\"(\r\n\t\"LocationIdentifier\", \"ArchiveDate\", \"DeviceId\", \"Table\", \"DataType\") VALUES (@LocationIdentifier, @ArchiveDate, @DeviceId, @Table, @DataType)";

            //                    //    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, targetConnection))
            //                    //    {
            //                    //        insertCommand.Parameters.AddWithValue("@LocationIdentifier", signalId);
            //                    //        insertCommand.Parameters.AddWithValue("@ArchiveDate", dateToRetrieve);
            //                    //        insertCommand.Parameters.AddWithValue("@DeviceId", locationDevicesDict[signalId]);
            //                    //        insertCommand.Parameters.AddWithValue("@DataType", "IndianaEvent");
            //                    //        insertCommand.Parameters.AddWithValue("@Table", compressedData);

            //                    //        Console.WriteLine($"Inserting data into target database for {signalId}...");
            //                    //        insertCommand.CommandTimeout = 60;
            //                    //        insertCommand.ExecuteNonQuery();
            //                    //        Console.WriteLine($"Table inserted successfully.");
            //                    //    }
            //                    //}
            //                }
            //            }
            //            if (archiveLogs.Count > 10)
            //            {
            //                using (var scope = serviceProvider.CreateScope())
            //                {
            //                    var context = scope.ServiceProvider.GetService<EventLogContext>();

            //                    if (context != null)
            //                    {
            //                        foreach (var archiveLog in archiveLogs)
            //                        {
            //                            context.CompressedEvents.Add(archiveLog);
            //                        }
            //                        context.SaveChanges();
            //                    }
            //                }
            //                archiveLogs.Clear();
            //            }
            //        }//);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    logger.LogError($"Error inserting data: {ex.Message}");
            //}

            Console.WriteLine("Execution completed.");
        }

        //static byte[] GZipCompressToByte(string str)
        //{
        //    var bytes = Encoding.UTF8.GetBytes(str);

        //    using (var stream = new MemoryStream())
        //    {
        //        using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
        //        {
        //            compressionStream.Write(bytes, 0, bytes.Length);
        //        }
        //        return stream.ToArray();
        //    }
        //}

        //public static byte[] SerializeAndCompress<T>(T data)
        //{
        //    var settings = new JsonSerializerSettings
        //    {
        //        TypeNameHandling = TypeNameHandling.Arrays,
        //        // Add Converters with StringEnumConverter to handle enum serialization as integers
        //        Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter { AllowIntegerValues = true } }
        //    };
        //    string json = JsonConvert.SerializeObject(data, settings);
        //    return GZipCompressToByte(json);
        //}

    }
}
