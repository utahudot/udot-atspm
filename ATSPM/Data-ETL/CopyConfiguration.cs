using ArchiveLogs.Data.Models;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data;
using System.Data.SqlClient;

public class CopyConfiguration : Command
{
    public CopyConfiguration() : base("copy-config", "Copy Config Data")
    {
        Handler = CommandHandler.Create<string, DateTime>(Execute);
    }

    private void Execute(string signalId, DateTime timeStamp)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetDBConnectionString");

        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");
        Console.WriteLine($"Press any key to continue or close the window");
        Console.ReadKey();

        using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
        using (SqlConnection targetConnection = new SqlConnection(targetConnectionString))
        {
            sourceConnection.Open();
            targetConnection.Open();

            var queries = new Dictionary<string, string>();
            queries.Add("Jurisdictions", "SELECT [Id], [JurisdictionName], [Mpo], [CountyParish], [OtherPartners] FROM Jurisdictions");
            queries.Add("Region", "Select [ID], [Description] FROM Region");
            queries.Add("Signals", "Select [VersionID],[SignalID],[Latitude],[Longitude],[PrimaryName],[SecondaryName],[IPAddress],[RegionID],[ControllerTypeID],[Enabled],[VersionActionId],[Note],[Start],[JurisdictionId],[Pedsare1to1] FROM [Signals] WHERE TRY_CAST(Latitude AS FLOAT) != 0.0");
            queries.Add("Approaches", "Select [ApproachID], [VersionID], [SignalID], [DirectionTypeID], [Description], [MPH], [ProtectedPhaseNumber], [IsProtectedPhaseOverlap], [PermissivePhaseNumber], [IsPermissivePhaseOverlap], [PedestrianPhaseNumber], [IsPedestrianPhaseOverlap], [PedestrianDetectors] from Approaches");
            queries.Add("Detectors", "Select [ID], [DetectorID], [DetChannel], [DistanceFromStopBar], [MinSpeedFilter], [DateAdded], [DateDisabled], [LaneNumber], [MovementTypeID], case when [LaneTypeID] IS null then 0 else LaneTypeID end as LaneTypeId,[DecisionPoint], [MovementDelay], [ApproachID], [DetectionHardwareID], [LatencyCorrection] From Detectors where MovementTypeID is not null");
            queries.Add("DetectionTypeMetricTypes", "Select [DetectionType_DetectionTypeID], [MetricType_MetricID] From DetectionTypeMetricTypes");
            queries.Add("DetectionTypeDetector", "Select DetectionTypeDetector.ID, DetectionTypeDetector.DetectionTypeId From Detectors Join DetectionTypeDetector on Detectors.ID = DetectionTypeDetector.ID where Detectors.MovementTypeID is not null");


            foreach (var query in queries)
            {
                using (SqlCommand sourceCommand = new SqlCommand(query.Value, sourceConnection))
                using (SqlDataReader reader = sourceCommand.ExecuteReader())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetConnection, SqlBulkCopyOptions.KeepIdentity, null))
                    {
                        switch (query.Key)
                        {
                            case "Signals":
                                bulkCopy.ColumnMappings.Add("SignalID", "SignalID");
                                bulkCopy.ColumnMappings.Add("VersionID", "VersionID");
                                bulkCopy.ColumnMappings.Add("Latitude", "Latitude");
                                bulkCopy.ColumnMappings.Add("Longitude", "Longitude");
                                bulkCopy.ColumnMappings.Add("PrimaryName", "PrimaryName");
                                bulkCopy.ColumnMappings.Add("SecondaryName", "SecondaryName");
                                bulkCopy.ColumnMappings.Add("IPAddress", "IPAddress");
                                bulkCopy.ColumnMappings.Add("RegionID", "RegionID");
                                bulkCopy.ColumnMappings.Add("ControllerTypeID", "ControllerTypeID");
                                bulkCopy.ColumnMappings.Add("Enabled", "Enabled");
                                bulkCopy.ColumnMappings.Add("VersionActionId", "VersionActionId");
                                bulkCopy.ColumnMappings.Add("Note", "Note");
                                bulkCopy.ColumnMappings.Add("Start", "Start");
                                bulkCopy.ColumnMappings.Add("JurisdictionId", "JurisdictionId");
                                bulkCopy.ColumnMappings.Add("Pedsare1to1", "Pedsare1to1");
                                break;
                            case "Approaches":
                                bulkCopy.ColumnMappings.Add("ApproachID", "ApproachID");
                                bulkCopy.ColumnMappings.Add("VersionID", "VersionID");
                                bulkCopy.ColumnMappings.Add("SignalID", "SignalID");
                                bulkCopy.ColumnMappings.Add("DirectionTypeID", "DirectionTypeID");
                                bulkCopy.ColumnMappings.Add("Description", "Description");
                                bulkCopy.ColumnMappings.Add("MPH", "MPH");
                                bulkCopy.ColumnMappings.Add("ProtectedPhaseNumber", "ProtectedPhaseNumber");
                                bulkCopy.ColumnMappings.Add("IsProtectedPhaseOverlap", "IsProtectedPhaseOverlap");
                                bulkCopy.ColumnMappings.Add("PermissivePhaseNumber", "PermissivePhaseNumber");
                                bulkCopy.ColumnMappings.Add("IsPermissivePhaseOverlap", "IsPermissivePhaseOverlap");
                                bulkCopy.ColumnMappings.Add("PedestrianPhaseNumber", "PedestrianPhaseNumber");
                                bulkCopy.ColumnMappings.Add("IsPedestrianPhaseOverlap", "IsPedestrianPhaseOverlap");
                                bulkCopy.ColumnMappings.Add("PedestrianDetectors", "PedestrianDetectors");
                                break;

                        }
                        //if (query.Key == "Approaches" || query.Key == "Signals")
                        //{
                        //    bulkCopy.ColumnMappings.Add("SignalID", "SignalID");
                        //}
                        bulkCopy.DestinationTableName = query.Key;
                        try
                        {
                            bulkCopy.WriteToServer(reader);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{query.Key}-{ex.Message}");
                        }
                    }
                }
                Console.WriteLine($"Data copied and bulk inserted for {query.Key}.");
            }
        }

        Console.WriteLine("Data copied and bulk inserted.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private DataTable ConvertSignalsToDataTable(List<ControllerEventLog> objectList)
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
