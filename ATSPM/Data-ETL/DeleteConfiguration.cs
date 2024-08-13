using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;

public class DeleteConfiguration : Command
{
    public DeleteConfiguration() : base("delete-config", "Delete Config Data")
    {
        Handler = CommandHandler.Create<string, DateTime>(Execute);
    }

    private void Execute(string signalId, DateTime timeStamp)
    {
        // Your code from the Main method goes here

        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        string targetConnectionString = config.GetConnectionString("TargetDBConnectionString");

        Console.WriteLine($"target is {targetConnectionString}");
        Console.WriteLine($"Press any key to continue otherwise close the window");
        Console.ReadKey();

        using (SqlConnection connection = new SqlConnection(targetConnectionString))
        {
            connection.Open();

            // Delete data from the specified tables
            string[] tablesToDelete = new string[]
            {
            "signals",
            "Approaches",
            "detectors",
            "DetectionTypeMetricTypes",
            "DetectionTypeDetector",
            "Region",
            "Jurisdictions"
            };

            foreach (string tableName in tablesToDelete)
            {
                using (SqlCommand deleteCommand = new SqlCommand($"DELETE FROM {tableName}", connection))
                {
                    deleteCommand.ExecuteNonQuery();
                    Console.WriteLine($"Deleted data from table: {tableName}");
                }
            }
        }

        // Add any additional code or logic as needed

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }


}
