using ArchiveLogs.Data.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data;
using System.Data.SqlClient;
using System.IO.Compression;

public class ExtractCommand : Command
{
    public ExtractCommand() : base("extract", "Extract logs command")
    {
        AddOption(new Option<string>("--signalid", "The signal ID"));
        AddOption(new Option<DateTime>("--timestamp", "The timestamp"));
        Handler = CommandHandler.Create<string, DateTime>(Execute);
    }

    private void Execute(string signalId, DateTime timeStamp)
    {
        // Your code from the Main method goes here
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetDBConnectionString");

        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"target is {targetConnectionString}");
        Console.WriteLine($"Press any key to continue otherwise close the window");
        Console.ReadKey();
        string selectQuery = $"SELECT LogData FROM [dbo].[ControllerLogArchives] where SignalId = {signalId} and ArchiveDate = '{timeStamp.Date}'";
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
                        var jsonObject = JsonConvert.DeserializeObject<List<ControllerEventLog>>(json);
                        Console.WriteLine($"Data deserialized...");

                        jsonObject.ForEach(x => x.SignalId = signalId);

                        var dataTable = ConvertToDataTable(jsonObject);

                        using (SqlConnection targetConnection = new SqlConnection(targetConnectionString))
                        {
                            Console.WriteLine($"Inserting data...");
                            targetConnection.Open();

                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetConnection))
                            {
                                bulkCopy.DestinationTableName = "Controller_Event_Log";
                                bulkCopy.WriteToServer(dataTable);
                            }
                            Console.WriteLine($"Data inserted...");
                        }
                    }
                }
            }
        }

        // Add any additional code or logic as needed

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private DataTable ConvertToDataTable(List<ControllerEventLog> objectList)
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

        foreach (var item in objectList.Distinct())
        {
            try
            {
                dataTable.Rows.Add(item.SignalId, item.Timestamp, item.EventCode, item.EventParam);
            }
            catch
            {
                // Ignore errors on insert
            }
        }

        return dataTable;
    }
}
