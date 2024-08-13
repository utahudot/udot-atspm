//using ArchiveLogs.Table.Models;
using ArchiveLogs.Data.Models;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;

public class CopyArchiveLog : Command
{
    public CopyArchiveLog() : base("copy", "Create Archive Log command")
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
        string targetConnectionString = config.GetConnectionString("TargetEventLogDBConnectionString");
        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");
        Console.WriteLine($"Press any key to continue or close the window");
        Console.ReadKey();

        string selectQuery = $"SELECT SignalId, ArchiveDate, LogData FROM [dbo].[ControllerLogArchives] where SignalId = {signalId} and ArchiveDate = '{timeStamp.Date}'";

        using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
        {
            sourceConnection.Open();

            List<ControllerLogArchive> eventLogs = new List<ControllerLogArchive>();

            using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
            {
                Console.WriteLine($"Executing query: {selectQuery}");
                using (SqlDataReader reader = selectCommand.ExecuteReader())
                {
                    using (SqlConnection targetConnection = new SqlConnection(targetConnectionString))
                    {
                        targetConnection.Open();
                        while (reader.Read())
                        {

                            string insertQuery = "INSERT INTO [dbo].[ControllerLogArchives] (SignalIdentifier, ArchiveDate, LogData) VALUES (@SignalIdentifier, @ArchiveDate, @LogData)";

                            using (SqlCommand insertCommand = new SqlCommand(insertQuery, targetConnection))
                            {
                                insertCommand.Parameters.AddWithValue("@SignalIdentifier", (string)reader["SignalId"]);
                                insertCommand.Parameters.AddWithValue("@ArchiveDate", (DateTime)reader["ArchiveDate"]);
                                insertCommand.Parameters.AddWithValue("@LogData", (byte[])reader["LogData"]);

                                Console.WriteLine($"Inserting data into target database...");
                                insertCommand.ExecuteNonQuery();
                                Console.WriteLine($"Data inserted successfully.");
                            }
                        }
                    }
                }


            }

            Console.WriteLine("Execution completed.");
        }

    }

}
