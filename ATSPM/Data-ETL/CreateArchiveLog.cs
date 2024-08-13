using ArchiveLogs.Data.Models;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

public class CreateCommand : Command
{
    public CreateCommand() : base("create", "Create Archive Log command")
    {
        AddOption(new Option<string>("--signalid", "The signal ID"));
        AddOption(new Option<DateTime>("--timestamp", "The timestamp"));
        Handler = CommandHandler.Create<string, DateTime>(Execute);
    }

    private void Execute(string signalId, DateTime timeStamp)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetAvenueDBConnectionString");
        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");
        Console.WriteLine($"Press any key to continue or close the window");
        Console.ReadKey();

        string selectQuery = $"SELECT SignalId, Timestamp, EventCode, EventParam FROM [dbo].[Controller_Event_Log] WHERE SignalId = '{signalId}' AND Timestamp between '{timeStamp.Date}' AND '{timeStamp.AddDays(1).Date}'";

        using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
        {
            sourceConnection.Open();

            List<ControllerEventLog> eventLogs = new List<ControllerEventLog>();

            using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
            {
                Console.WriteLine($"Executing query: {selectQuery}");
                using (SqlDataReader reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ControllerEventLog eventLog = new ControllerEventLog()
                        {
                            Timestamp = (DateTime)reader["Timestamp"],
                            EventCode = (short)reader["EventCode"],
                            EventParam = (short)reader["EventParam"]
                        };

                        eventLogs.Add(eventLog);
                    }
                }
            }
            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                ControllerLogArchive archiveLog = new ControllerLogArchive()
                {
                    SignalId = signalId,
                    ArchiveDate = timeStamp.Date,
                    LogData = eventLogs
                };

                byte[] compressedData = GZipCompressToByte(JsonSerializer.Serialize(archiveLog.LogData.Select(c => new { c.SignalId, c.EventCode, c.EventParam, c.Timestamp }), new JsonSerializerOptions()));


                //Console.WriteLine($"Compressing data...");
                //using (MemoryStream memoryStream = new MemoryStream())
                //{
                //    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                //    {
                //        using (StreamWriter writer = new StreamWriter(gzipStream, Encoding.UTF8))
                //        {
                //            var options = new JsonSerializerOptions
                //            {

                //            };

                //            var jsonString = JsonSerializer.Serialize(archiveLog.LogData, options);
                //            writer.Write(jsonString);
                //        }
                //    }

                //    compressedData = memoryStream.ToArray();
                //}

                using (SqlConnection targetConnection = new SqlConnection(targetConnectionString))
                {
                    targetConnection.Open();

                    string insertQuery = "INSERT INTO [dbo].[ControllerLogArchives] (SignalIdentifier, ArchiveDate, LogData) VALUES (@SignalIdentifier, @ArchiveDate, @LogData)";

                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, targetConnection))
                    {
                        insertCommand.Parameters.AddWithValue("@SignalIdentifier", archiveLog.SignalId);
                        insertCommand.Parameters.AddWithValue("@ArchiveDate", archiveLog.ArchiveDate);
                        insertCommand.Parameters.AddWithValue("@LogData", compressedData);

                        Console.WriteLine($"Inserting data into target database...");
                        insertCommand.ExecuteNonQuery();
                        Console.WriteLine($"Data inserted successfully.");
                    }
                }
            }
        }

        Console.WriteLine("Execution completed.");
    }

    static byte[] GZipCompressToByte(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);

        using (var stream = new MemoryStream())
        {
            using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
            {
                compressionStream.Write(bytes, 0, bytes.Length);
            }
            return stream.ToArray();
        }
    }

}
