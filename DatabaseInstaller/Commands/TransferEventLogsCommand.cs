#region license
// Copyright 2024 Utah Departement of Transportation
// for DatabaseInstaller - %Namespace%/TransferEventLogsCommand.cs
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


//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Npgsql;
//using System.Collections.Generic;
//using System;
//using System.CommandLine;
//using System.CommandLine.NamingConventionBinder;
//using System.Data.SqlClient;
//using System.IO;
//using System.IO.Compression;
//using System.Text;
//using System.Threading.Tasks;
//using Utah.Udot.Atspm.Data;
//using Utah.Udot.Atspm.Data.Models;
//using Utah.Udot.Atspm.Data.Models.EventLogModels;
//using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

//public class Command : Command
//{
//    private readonly ILogger<Command> logger;
//    private readonly IServiceProvider serviceProvider;
//    private readonly ILocationRepository locationRepository;

//    public Command(
//        ILogger<Command> logger,
//        IServiceProvider serviceProvider,
//        ILocationRepository locationRepository) : base("transfer-logs", "Transfer event logs from old format to new")
//    {
//        var dateOption = new Option<DateTime?>("--date", "The date");
//        dateOption.IsRequired = false; // Make the option not required
//        AddOption(dateOption);

//        var signalOption = new Option<string?>("--signalid", "Location Identifier");
//        signalOption.IsRequired = false; // Make the option not required
//        AddOption(signalOption);

//        Handler = CommandHandler.Create<DateTime?, string?>(ExecuteAsync);
//        this.logger = logger;
//        this.serviceProvider = serviceProvider;
//        this.locationRepository = locationRepository;
//    }

//    private async Task ExecuteAsync(DateTime? date, string? signalId)
//    {
//        IConfiguration config = new ConfigurationBuilder()
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile("appsettings.json")
//            .Build();

//        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
//        string targetConnectionString = config.GetConnectionString("TargetEventLogDBConnectionString");
//        Console.WriteLine($"Source is {sourceConnectionString}");
//        Console.WriteLine($"Target is {targetConnectionString}");

//        var dateToRetrieve = date == null ? DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) : DateOnly.FromDateTime(date.Value);

//        var jurisdictionIds = config.GetSection("JurisdictionIds");
//        var jurisdictionIdsMapping = new Dictionary<int, int>();
//        jurisdictionIds.Bind(jurisdictionIdsMapping);

//        var locations = new List<Location>();
//        if (signalId == null)
//        {
//            locations = locationRepository.GetLatestVersionOfAllLocations(dateToRetrieve.ToDateTime(new TimeOnly())).Where(l => jurisdictionIdsMapping.Values.ToList().Contains(l.JurisdictionId.Value)).ToList();
//        }
//        //else
//        //{
//        //    var location = locationRepository.GetLocationWithDevice(signalId, dateToRetrieve.ToDateTime(new TimeOnly()));
//        //    locations.Add(location);
//        //}
//        DeleteOldData(config, targetConnectionString, locations);

//        try
//        {

//            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
//            {
//                sourceConnection.Open();
//                var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
//                //Parallel.ForEach(signals, options, location =>
//                var archiveLogs = new List<CompressedEventLogs<IndianaEvent>>();
//                foreach (var location in locations)
//                {
//                    GetLogs(dateToRetrieve, sourceConnection, archiveLogs, location, config);
//                    if (archiveLogs.Count > 10)
//                    {
//                        InsertLogs(archiveLogs);
//                    }
//                }//);
//                if (archiveLogs.Count > 0)
//                {
//                    InsertLogs(archiveLogs);
//                }
//            }
//        }
//        catch (System.Exception ex)
//        {
//            logger.LogError($"Error inserting data: {ex.Message}");
//        }

//        Console.WriteLine("Execution completed.");
//    }

//    private void InsertLogs(List<CompressedEventLogs<IndianaEvent>> archiveLogs)
//    {
//        using (var scope = serviceProvider.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetService<EventLogContext>();

//            if (context != null)
//            {
//                foreach (var archiveLog in archiveLogs)
//                {
//                    context.CompressedEvents.Add(archiveLog);
//                }
//                context.SaveChanges();
//            }
//        }
//        archiveLogs.Clear();
//    }

//    private static void GetLogs(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedEventLogs<IndianaEvent>> archiveLogs, Location? location, IConfiguration config)
//    {
//        var prependIdentifier = config.GetValue<string>("PrependIdentifier").Replace("'", "");
//        var locationIdentifier = location.LocationIdentifier;
//        if (!String.IsNullOrEmpty(prependIdentifier))
//        {
//            locationIdentifier = locationIdentifier.Replace(prependIdentifier, "");
//        }
//        string selectQuery = $"SELECT SignalId, Timestamp, EventCode, EventParam FROM [dbo].[Controller_Event_Log] WHERE SignalId = '{locationIdentifier}' AND Timestamp between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";


//        var idRange = config.GetValue<int>("IdRange");

//        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
//        {
//            List<IndianaEvent> eventLogs = new List<IndianaEvent>();
//            selectCommand.CommandTimeout = 120;
//            Console.WriteLine($"Executing query: {selectQuery}");
//            using (SqlDataReader reader = selectCommand.ExecuteReader())
//            {
//                while (reader.Read())
//                {
//                    try
//                    {
//                        IndianaEvent eventLog = new IndianaEvent()
//                        {
//                            Timestamp = (DateTime)reader["Timestamp"],
//                            EventCode = Convert.ToInt16((int)reader["EventCode"]),
//                            EventParam = Convert.ToInt16((int)reader["EventParam"])
//                        };

//                        eventLogs.Add(eventLog);
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($" Event: {location}-{reader["Timestamp"]} EventCode:{reader["EventCode"]} EventParam:{reader["EventParam"]} Error reading record: {ex.Message}");
//                    }
//                }
//            }

//            Console.WriteLine($"Controller Event Logs retrieved");
//            if (eventLogs.Count > 0)
//            {
//                var device = location.Devices.FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SignalController);
//                if (device != null)
//                {
//                    CompressedEventLogs<IndianaEvent> archiveLog = new CompressedEventLogs<IndianaEvent>()
//                    {
//                        LocationIdentifier = location.LocationIdentifier,
//                        DeviceId = device.Id + idRange,
//                        ArchiveDate = dateToRetrieve,
//                        Data = eventLogs
//                    };
//                    archiveLogs.Add(archiveLog);
//                }
//                else
//                {
//                    Console.WriteLine($"No device found for signal {location.LocationIdentifier}");
//                }

//            }
//        }
//    }

//    private void DeleteOldData(IConfiguration config, string targetConnectionString, IEnumerable<Location> locations)
//    {
//        var delete = config.GetValue<bool>("Delete");
//        var prependIdentifier = config.GetValue<string>("PrependIdentifier").Replace("'", "");
//        if (!delete)
//        {
//            return;
//        }
//        try
//        {
//            var deleteCommand = config.GetSection("DeleteCommand").Value + " And \"LocationIdentifier\" in ('" + string.Join($"', '", locations.Select(l => l.LocationIdentifier)) + "')";

//            using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
//            {
//                targetConnection.Open();

//                using (NpgsqlCommand command = new NpgsqlCommand(deleteCommand, targetConnection))
//                {
//                    Console.WriteLine($"Removing old records");
//                    command.CommandTimeout = 60;
//                    command.ExecuteNonQuery();
//                    Console.WriteLine($"Data deleted successfully.");
//                }
//            }
//        }
//        catch (System.Exception ex)
//        {
//            logger.LogError($"Error deleting old records: {ex.Message}");
//        }
//    }

//    static byte[] GZipCompressToByte(string str)
//    {
//        var bytes = Encoding.UTF8.GetBytes(str);

//        using (var stream = new MemoryStream())
//        {
//            using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
//            {
//                compressionStream.Write(bytes, 0, bytes.Length);
//            }
//            return stream.ToArray();
//        }
//    }

//    public static byte[] SerializeAndCompress<T>(T data)
//    {
//        var settings = new JsonSerializerSettings
//        {
//            TypeNameHandling = TypeNameHandling.Arrays,
//            // Add Converters with StringEnumConverter to handle enum serialization as integers
//            Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter { AllowIntegerValues = true } }
//        };
//        string json = JsonConvert.SerializeObject(data, settings);
//        return GZipCompressToByte(json);
//    }

//}
