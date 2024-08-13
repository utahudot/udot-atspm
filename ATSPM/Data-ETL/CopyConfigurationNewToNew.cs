using ArchiveLogs.Data.Models;
using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data;
using System.Data.SqlClient;

public class CopyConfigurationNewNew : Command
{
    public CopyConfigurationNewNew() : base("copy-config-newnew", "Copy Config Data for new data format")
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
        string targetConnectionString = config.GetConnectionString("NewTargetDBConnectionString");

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
            queries.Add("Jurisdictions", "SELECT [Id], [Name], [Mpo], [CountyParish], [OtherPartners] FROM Jurisdictions");
            queries.Add("Regions", "Select [Id], [Description] FROM Regions");
            queries.Add("Signals", "Select [Id],[SignalIDentifier],[Latitude],[Longitude],[PrimaryName],[SecondaryName],[IPAddress],[RegionId],[ControllerTypeId],[ChartEnabled],  1 AS LoggingEnabled,[VersionActionId],[Note],[Start],[JurisdictionId],[Pedsare1to1] FROM [Signals] WHERE TRY_CAST(Latitude AS FLOAT) != 0.0");
            queries.Add("Approaches", "Select [Id], [SignalId], [DirectionTypeId], [Description], [MPH], [ProtectedPhaseNumber], [IsProtectedPhaseOverlap], [PermissivePhaseNumber], [IsPermissivePhaseOverlap], [PedestrianPhaseNumber], [IsPedestrianPhaseOverlap], [PedestrianDetectors] from Approaches");
            queries.Add("Detectors", "Select [Id], [DectectorIdentifier], [DetChannel], [DistanceFromStopBar], [MinSpeedFilter], [DateAdded], [DateDisabled], [LaneNumber], [MovementTypeId], case when [LaneTypeId] IS null then 0 else LaneTypeId end as LaneTypeId,[DecisionPoint], [MovementDelay], [ApproachId], [DetectionHardwareId], [LatencyCorrection] From Detectors where MovementTypeId is not null");
            queries.Add("DetectionTypeMetricType", "Select [DetectionTypeDetectionTypesId], [MetricTypeMetricsId] From DetectionTypeMetricType");
            queries.Add("DetectionTypeDetector", "Select DetectionTypeDetector.DetectionTypesId, DetectionTypeDetector.DetectorsId From Detectors Join DetectionTypeDetector on Detectors.Id = DetectionTypeDetector.DetectorsId where Detectors.MovementTypeId is not null");

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
                                bulkCopy.ColumnMappings.Add("SignalIdentifier", "SignalIdentifier");
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("Latitude", "Latitude");
                                bulkCopy.ColumnMappings.Add("Longitude", "Longitude");
                                bulkCopy.ColumnMappings.Add("PrimaryName", "PrimaryName");
                                bulkCopy.ColumnMappings.Add("SecondaryName", "SecondaryName");
                                bulkCopy.ColumnMappings.Add("IPAddress", "Ipaddress");
                                bulkCopy.ColumnMappings.Add("RegionId", "RegionId");
                                bulkCopy.ColumnMappings.Add("ControllerTypeId", "ControllerTypeId");
                                bulkCopy.ColumnMappings.Add("ChartEnabled", "ChartEnabled");
                                bulkCopy.ColumnMappings.Add("LoggingEnabled", "LoggingEnabled");
                                bulkCopy.ColumnMappings.Add("VersionActionId", "VersionActionId");
                                bulkCopy.ColumnMappings.Add("Note", "Note");
                                bulkCopy.ColumnMappings.Add("Start", "Start");
                                bulkCopy.ColumnMappings.Add("JurisdictionId", "JurisdictionId");
                                bulkCopy.ColumnMappings.Add("Pedsare1to1", "Pedsare1to1");
                                break;
                            case "Approaches":
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("SignalId", "SignalId");
                                bulkCopy.ColumnMappings.Add("DirectionTypeId", "DirectionTypeId");
                                bulkCopy.ColumnMappings.Add("Description", "Description");
                                bulkCopy.ColumnMappings.Add("MPH", "Mph");
                                bulkCopy.ColumnMappings.Add("ProtectedPhaseNumber", "ProtectedPhaseNumber");
                                bulkCopy.ColumnMappings.Add("IsProtectedPhaseOverlap", "IsProtectedPhaseOverlap");
                                bulkCopy.ColumnMappings.Add("PermissivePhaseNumber", "PermissivePhaseNumber");
                                bulkCopy.ColumnMappings.Add("IsPermissivePhaseOverlap", "IsPermissivePhaseOverlap");
                                bulkCopy.ColumnMappings.Add("PedestrianPhaseNumber", "PedestrianPhaseNumber");
                                bulkCopy.ColumnMappings.Add("IsPedestrianPhaseOverlap", "IsPedestrianPhaseOverlap");
                                bulkCopy.ColumnMappings.Add("PedestrianDetectors", "PedestrianDetectors");
                                break;
                            case "Jurisdictions":
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("Name", "Name");
                                bulkCopy.ColumnMappings.Add("Mpo", "Mpo");
                                bulkCopy.ColumnMappings.Add("CountyParish", "CountyParish");
                                bulkCopy.ColumnMappings.Add("OtherPartners", "OtherPartners");
                                break;
                            case "Regions":
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("Description", "Description");
                                break;
                            case "Detectors":
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("DectectorIdentifier", "DectectorIdentifier");
                                bulkCopy.ColumnMappings.Add("DetChannel", "DetChannel");
                                bulkCopy.ColumnMappings.Add("DistanceFromStopBar", "DistanceFromStopBar");
                                bulkCopy.ColumnMappings.Add("MinSpeedFilter", "MinSpeedFilter");
                                bulkCopy.ColumnMappings.Add("DateAdded", "DateAdded");
                                bulkCopy.ColumnMappings.Add("DateDisabled", "DateDisabled");
                                bulkCopy.ColumnMappings.Add("LaneNumber", "LaneNumber");
                                bulkCopy.ColumnMappings.Add("MovementTypeId", "MovementTypeId");
                                bulkCopy.ColumnMappings.Add("LaneTypeId", "LaneTypeId");
                                bulkCopy.ColumnMappings.Add("DecisionPoint", "DecisionPoint");
                                bulkCopy.ColumnMappings.Add("MovementDelay", "MovementDelay");
                                bulkCopy.ColumnMappings.Add("ApproachId", "ApproachId");
                                bulkCopy.ColumnMappings.Add("DetectionHardwareId", "DetectionHardwareId");
                                bulkCopy.ColumnMappings.Add("LatencyCorrection", "LatencyCorrection");
                                break;
                            case "DetectionTypeMetricTypes":
                                bulkCopy.ColumnMappings.Add("DetectionTypeDetectionTypesId", "DetectionTypeDetectionTypesId");
                                bulkCopy.ColumnMappings.Add("MetricTypeMetricsId", "MetricTypeMetricsId");
                                break;
                            case "DetectionTypeDetector":
                                bulkCopy.ColumnMappings.Add("DetectionTypesId", "DetectionTypesId");
                                bulkCopy.ColumnMappings.Add("DetectorsId", "DetectorsId");
                                break;
                        }

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
        dataTable.Columns.Add("SignalId", typeof(string));
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
