using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;
using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;

public class Sync : Command
{
    private readonly ILogger<CopyConfigurationNewPostgres> logger;
    private readonly IJurisdictionRepository jurisdictionRepository;
    private readonly ILocationTypeRepository locationTypeRepository;
    private readonly ILocationRepository locationRepository;

    public Sync(
        ILogger<CopyConfigurationNewPostgres> logger,
        IJurisdictionRepository jurisdictionRepository,
        ILocationTypeRepository locationTypeRepository,
        ILocationRepository locationRepository

        ) : base("sync", "Sync Config Data for new data format in a posgres database")
    {
        Handler = CommandHandler.Create<string, DateTime>(Execute);
        this.logger = logger;
        this.jurisdictionRepository = jurisdictionRepository;
        this.locationTypeRepository = locationTypeRepository;
        this.locationRepository = locationRepository;
    }

    private void Execute(string signalId, DateTime timeStamp)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var jurisdictionIds = config.GetSection("JurisdictionIds").Get<List<int>>();
        var jurisdictions = jurisdictionRepository.GetList().Where(j => jurisdictionIds.Contains(j.Id));
        foreach (var jurisdiction in jurisdictions)
        {
            var LocationsForTarget = locationRepository.GetList()
                .Include(s => s.Devices)
                .Include(s => s.Jurisdiction)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.DirectionType)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectorComments)
                .Include(s => s.Approaches)
                    .ThenInclude(a => a.Detectors)
                        .ThenInclude(d => d.DetectionTypes)
                            .ThenInclude(d => d.MeasureTypes)
                .Where(Location => Location.Jurisdiction.Id == jurisdiction.Id)
                .ToList();
        }


        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetConfigDBConnectionString");

        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");
        AddLocationTypes();

        using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
        {
            sourceConnection.Open();

            var queries = new Dictionary<string, string>();

            queries.Add("Jurisdictions", "SELECT [Id], [JurisdictionName], [Mpo], [CountyParish], [OtherPartners] FROM Jurisdictions");
            queries.Add("Region", "Select [ID], [Description] FROM Region");
            queries.Add("Areas", "SELECT Id,\r\n       AreaName\r\nFROM Areas");

            queries.Add("Locations", "SELECT VersionID,\r\n       SignalId,\r\n       PrimaryName,\r\n       SecondaryName,\r\n       1,\r\n       Enabled,\r\n       JurisdictionId,\r\n\t   ISNULL(TRY_CONVERT(float, Latitude),0) as Latitude,\r\n\t   ISNULL(TRY_CONVERT(float, Longitude),0) as Longitude,\r\n       Pedsare1to1,\r\n       RegionId,\r\n       Start,\r\n       VersionActionId, 1 as LocationTypeId\r\n FROM [srwtcmoedb].[moe].dbo.Signals");

            queries.Add("Products", "SELECT [ControllerTypeID],[Description] as Manufacturer, [Description] as Model  FROM [MOE].[dbo].[ControllerTypes]");
            queries.Add("DeviceConfigurations", "SELECT [ControllerTypeID], [ControllerTypeID] as ProductId, [Description], 'FTPS' as Protocol, SNMPPort, FTPDirectory, [UserName], [Password]  FROM [MOE].[dbo].[ControllerTypes]");
            queries.Add("Devices", "Select 'Active' as Status, [VersionID] as LocationId, [VersionID],[SignalID],[Latitude],[Longitude],[PrimaryName],[SecondaryName],'127.0.0.1' as IPAddress,[RegionID],[ControllerTypeID],[Enabled],  1 AS LoggingEnabled,[VersionActionId],[Note],[Start],[JurisdictionId],[Pedsare1to1], 1 as LocationTypeId,'SignalController' as DeviceType FROM [Signals] WHERE TRY_CAST(Latitude AS FLOAT) != 0.0");
            queries.Add("SpeedDevices", "Select 'Active' as Status, s.[VersionID] as LocationId, s.[VersionID],s.[SignalID],[Latitude],[Longitude],[PrimaryName],[SecondaryName],'127.0.0.1' as IPAddress,[RegionID],[ControllerTypeID],[Enabled],  1 AS LoggingEnabled,[VersionActionId],[Note],[Start],[JurisdictionId],[Pedsare1to1], 1 as LocationTypeId, 'WavetronixSpeed' as DeviceType FROM [Signals] s Join [dbo].[Approaches] a on s.VersionID = a.VersionID Join [dbo].[Detectors] d on a.ApproachID = d.ApproachID Join [dbo].[DetectionTypeDetector] dtd on d.ID = dtd.ID WHERE TRY_CAST(Latitude AS FLOAT) != 0.0 And dtd.DetectionTypeID = 3 group by s.[VersionID], s.[VersionID],s.[SignalID],[Latitude],[Longitude],[PrimaryName],[SecondaryName], IPAddress,[RegionID],[ControllerTypeID],[Enabled],[VersionActionId],[Note],[Start],[JurisdictionId],[Pedsare1to1]");

            queries.Add("AreaLocation", "SELECT Area_Id,\r\n       Signal_VersionID\r\nFROM AreaSignals");

            queries.Add("Approaches", "Select a.ApproachID, a.VersionId, a.DirectionTypeID, a.Description, a.MPH, a.ProtectedPhaseNumber, a.IsProtectedPhaseOverlap, a.PermissivePhaseNumber, a.IsPermissivePhaseOverlap, a.PedestrianPhaseNumber, a.IsPedestrianPhaseOverlap, a.PedestrianDetectors from Approaches a join Signals s on s.VersionID = a.VersionID  WHERE TRY_CAST(Latitude AS FLOAT) != 0.0");
            queries.Add("Detectors", "SELECT d.[ID],d.[DetectorID],d.[DetChannel],d.[DistanceFromStopBar],d.[MinSpeedFilter],d.[DateAdded],d.[DateDisabled],d.[LaneNumber],COALESCE(d.[MovementTypeID], 6) AS MovementTypeID,COALESCE(d.[LaneTypeID], 1) AS LaneTypeID,d.[DecisionPoint],d.[MovementDelay],d.[ApproachID],d.[DetectionHardwareID],d.[LatencyCorrection] FROM [MOE].[dbo].[Detectors] d Join Approaches a on d.ApproachID = a.ApproachID Join Signals s on a.VersionID = s.VersionID WHERE TRY_CAST(Latitude AS FLOAT) != 0.0 and MovementTypeID is not null");
            queries.Add("DetectionTypeMetricTypes", "Select [DetectionType_DetectionTypeID], [MetricType_MetricID] From DetectionTypeMetricTypes");
            queries.Add("DetectionTypeDetector", "Select DetectionTypeDetector.DetectionTypeId, DetectionTypeDetector.ID \r\nFrom Detectors d\r\nJoin DetectionTypeDetector on d.ID = DetectionTypeDetector.ID \r\n Join Approaches a on d.ApproachID = a.ApproachID\r\n  Join Signals s on a.VersionID = s.VersionID\r\n  WHERE TRY_CAST(Latitude AS FLOAT) != 0.0\r\nand d.MovementTypeID is not null");
            //queries.Add("Routes", "SELECT [Id] ,[RouteName] FROM [MOE].[dbo].[Routes]");
            //queries.Add("RouteSignals", "SELECT r.[Id]\r\n      ,[RouteId]\r\n      ,[Order]\r\n      ,[LocationIdentifier]\r\n\t  ,(SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) as PrimaryPhase\r\n  ,(SELECT DirectionTypeId\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) as PrimaryDirectionId\r\n   ,(SELECT IsOverlap\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) as IsPrimaryOverlap\r\n  ,(SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) as OpposingPhase\r\n  ,(SELECT DirectionTypeId\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) as OpposingDirectionId\r\n  ,(SELECT IsOverlap\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) as IsOpposingOverlap\r\n  FROM [MOE].[dbo].[RouteSignals] r\r\n  Where (SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) Is Not NULL And (SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) Is Not NULL");






            // Define column mappings for each table
            var columnMappings = new Dictionary<string, Dictionary<string, string>>
    {
        {
            "DeviceConfigurations", new Dictionary<string, string>
            {
                { "ControllerTypeID", "Id" },
                { "Description", "Firmware" },
                { "Protocol", "Protocol" },
                { "SNMPPort", "Port" },
                { "FTPDirectory", "Directory" },
                { "UserName", "UserName" },
                { "Password", "Password" },
                { "ProductId", "ProductId" }
            }
                },
        {
            "Products", new Dictionary<string, string>
            {
                { "ControllerTypeID", "Id" },
                { "Manufacturer", "Manufacturer" },
                { "Model", "Model" }
            }
                },
                { "Signals", new Dictionary<string, string>
            {
                { "SignalID", "LocationIdentifier" },
                { "VersionID", "Id" },
                { "Latitude", "Latitude" },
                { "Longitude", "Longitude" },
                { "PrimaryName", "PrimaryName" },
                { "SecondaryName", "SecondaryName" },
                { "IPAddress", "Ipaddress" },
                { "RegionID", "RegionId" },
                { "ControllerTypeID", "ControllerTypeId" },
                { "Enabled", "ChartEnabled" },
                { "LoggingEnabled", "LoggingEnabled" },
                { "VersionActionId", "VersionAction" },
                { "Note", "Note" },
                { "Start", "Start" },
                { "JurisdictionId", "JurisdictionId" },
                { "Pedsare1to1", "Pedsare1to1" }
            }
                },
        { "Locations", new Dictionary<string, string>
            {
                { "SignalID", "LocationIdentifier" },
                { "VersionID", "Id" },
                { "Latitude", "Latitude" },
                { "Longitude", "Longitude" },
                { "PrimaryName", "PrimaryName" },
                { "SecondaryName", "SecondaryName" },
                { "RegionId", "RegionId" },
                { "Enabled", "ChartEnabled" },
                { "VersionActionId", "VersionAction" },
                { "Start", "Start" },
                { "JurisdictionId", "JurisdictionId" },
                { "PedsAre1to1", "PedsAre1to1" },
                { "LocationTypeId", "LocationTypeId" }
            }
                },
        { "Devices", new Dictionary<string, string>
            {
                { "LoggingEnabled", "LoggingEnabled" },
                { "Status", "DeviceStatus" },
                { "IPAddress", "Ipaddress" },
                { "ControllerTypeID", "DeviceConfigurationId" },
                { "LocationId", "LocationId" },
                { "Note", "Notes" },
                { "DeviceType", "DeviceType" }
            }
                },
        { "SpeedDevices", new Dictionary<string, string>
            {
                { "LoggingEnabled", "LoggingEnabled" },
                { "Status", "DeviceStatus" },
                { "IPAddress", "Ipaddress" },
                { "ControllerTypeID", "DeviceConfigurationId" },
                { "LocationId", "LocationId" },
                { "Note", "Notes" },
                { "DeviceType", "DeviceType" }
            }
                },
        {
            "ControllerTypes", new Dictionary<string, string>
            {
                { "ControllerTypeID", "Id" },
                { "Description", "Product" },
                { "SNMPPort", "Port" },
                { "FTPDirectory", "Directory" },
                { "UserName", "UserName" },
                { "Password", "Password" },
            }
        },
        {
            "Approaches", new Dictionary<string, string>
            {
                { "ApproachID", "Id" },
                { "VersionId", "LocationId" },
                { "DirectionTypeID", "DirectionTypeId" },
                { "Description", "Description" },
                { "MPH", "Mph" },
                { "ProtectedPhaseNumber", "ProtectedPhaseNumber" },
                { "IsProtectedPhaseOverlap", "IsProtectedPhaseOverlap" },
                { "PermissivePhaseNumber", "PermissivePhaseNumber" },
                { "IsPermissivePhaseOverlap", "IsPermissivePhaseOverlap" },
                { "PedestrianPhaseNumber", "PedestrianPhaseNumber" },
                { "IsPedestrianPhaseOverlap", "IsPedestrianPhaseOverlap" },
                { "PedestrianDetectors", "PedestrianDetectors" }
            }
        },
        {
            "Jurisdictions", new Dictionary<string, string>
            {
                { "Id", "Id" },
                { "JurisdictionName", "Name" },
                { "Mpo", "Mpo" },
                { "CountyParish", "CountyParish" },
                { "OtherPartners", "OtherPartners" }
            }
        },
        {
            "Region", new Dictionary<string, string>
            {
                { "ID", "Id" },
                { "Description", "Description" }
            }
        },
        {
            "Areas", new Dictionary<string, string>
            {
                { "Id", "Id" },
                { "AreaName", "Name" }
            }
        },
        {
            "AreaLocation", new Dictionary<string, string>
            {
                { "Area_Id", "AreasId" },
                { "Signal_VersionID", "LocationsId" }
            }
        },
        {
            "Detectors", new Dictionary<string, string>
            {
                    { "ID", "Id"},
                    {"DetectorID", "DectectorIdentifier"},
                    {"DetChannel", "DetectorChannel"},
                    {"DistanceFromStopBar", "DistanceFromStopBar"},
                    {"MinSpeedFilter", "MinSpeedFilter"},
                    {"DateAdded", "DateAdded"},
                    {"DateDisabled", "DateDisabled"},
                    {"LaneNumber", "LaneNumber"},
                    {"MovementTypeID", "MovementType"},
                    {"LaneTypeId", "LaneType"},
                    {"DecisionPoint", "DecisionPoint"},
                    {"MovementDelay", "MovementDelay"},
                    {"ApproachID", "ApproachId"},
                    {"DetectionHardwareID", "DetectionHardware"},
                    { "LatencyCorrection", "LatencyCorrection"}
            }
        },
        {
            "DetectionTypeMetricTypes", new Dictionary<string, string>
            {
                {"DetectionType_DetectionTypeID", "DetectionTypesId"},
                {"MetricType_MetricID", "MeasureTypesId"}
            }
        },
        {
            "DetectionTypeDetector",  new Dictionary<string, string>
            {
                {"DetectionTypeId", "DetectionTypesId"},
                {"ID", "DetectorsId"}
            }
        },
        {
            "Routes",  new Dictionary<string, string>
            {
                {"Id", "Id"},
                {"RouteName", "Name"}
            }
        },
        {
            "RouteSignals",  new Dictionary<string, string>
            {
                {"Id", "Id"},
                {"RouteId", "RouteId"},
                {"Order", "Order"},
                {"SignalId", "LocationIdentifier"},
                {"PrimaryPhase", "PrimaryPhase"},
                {"OpposingPhase", "OpposingPhase"},
                {"PrimaryDirectionId", "PrimaryDirectionId"},
                {"OpposingDirectionId", "OpposingDirectionId"},
                {"IsPrimaryOverlap", "IsPrimaryOverlap"},
                {"IsOpposingOverlap", "IsOpposingOverlap"}
            }
        }
    };

            var tableNameMappings = new Dictionary<string, string>
    {
        {"Products", "Products" },
        {"DeviceConfigurations", "DeviceConfigurations" },
        {"Region", "Regions" },
        {"DetectionTypeMetricTypes", "DetectionTypeMeasureType" },
        {"Locations", "Locations"},
        {"Signals", "Devices"},
        {"Approaches", "Approaches"},
        {"Jurisdictions", "Jurisdictions"},
        {"Detectors", "Detectors"},
        {"DetectionTypeDetector", "DetectionTypeDetector"},
        {"ControllerTypes", "ControllerTypes"},
        {"Routes", "Routes"},
        {"Devices", "Devices"},
        {"SpeedDevices", "Devices"},
        {"Areas", "Areas"},
        {"AreaLocation", "AreaLocation"},
        {"RouteSignals", "RouteLocations"}
    };


            try
            {
                //UpdateStaticData(targetConnectionString);

                foreach (var query in queries)
                {
                    using (SqlCommand sourceCommand = new SqlCommand(query.Value, sourceConnection))
                    using (SqlDataReader reader = sourceCommand.ExecuteReader())
                    {
                        string targetTableName = "\"" + tableNameMappings[query.Key] + "\"";
                        DeleteDataFromTargetTable(query.Key, targetTableName, targetConnectionString);
                        ExportToCsvAndImportToPostgres(reader, targetTableName, targetConnectionString, columnMappings[query.Key]);
                    }
                    Console.WriteLine($"Data copied and bulk inserted for {query.Key}.");
                }
                DeleteDataFromTargetTable("", "\"" + "DetectionTypeMeasureType" + "\"", targetConnectionString);
                ImportCsvToPostgres("./Files/DetectionTypeMeasureType.csv", "\"" + "DetectionTypeMeasureType" + "\"", targetConnectionString, columnMappings["DetectionTypeMetricTypes"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, "Error copying data from SQL Server to Postgres");
                Console.WriteLine(ex.Message);
            }
            ResetSequences(targetConnectionString);
            ManuallyAddJurisdiction();
            //ManuallyAddPortlandConfiguration();
        }

        Console.WriteLine("Data copied and bulk inserted.");
    }

    private void ResetSequences(string targetConnectionString)
    {
        string[] sequences = new string[]
        {
        "\"Locations_Id_seq\"",
        "\"Jurisdictions_Id_seq\"",
        "\"Approaches_Id_seq\"",
        "\"Detectors_Id_seq\"",
        "\"Products_Id_seq\"",
        "\"DeviceConfigurations_Id_seq\"",
        "\"Regions_Id_seq\"",
        "\"Devices_Id_seq\"",
        "\"Routes_Id_seq\"",
        "\"Areas_Id_seq\"",
        "\"RouteLocations_Id_seq\""
        };

        using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
        {
            targetConnection.Open();
            foreach (string sequence in sequences)
            {
                // Correctly format table name derived from sequence name
                string tableName = sequence.Replace("_Id_seq\"", "").Replace("\"", "");

                string query = $"SELECT setval('public.{sequence}', (SELECT COALESCE(MAX(\"Id\"), 0) FROM public.\"{tableName}\") + 1);";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, targetConnection))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Sequence {sequence} reset successfully.");
                }
            }
        }
    }


    private void AddLocationTypes()
    {
        if (locationTypeRepository.GetList().Any())
        {
            return;
        }
        locationTypeRepository.Add(new LocationType { Name = "Intersection" });
    }

    private void ManuallyAddPortlandConfiguration()
    {
        throw new NotImplementedException();
    }

    private void ManuallyAddJurisdiction()
    {
        jurisdictionRepository.Add(new Jurisdiction { Name = "City of Portland", Mpo = "PBOT" });
    }

    private void UpdateStaticData(string targetConnectionString)
    {
        string insertQuery = "INSERT INTO public.\"LocationTypes\"(\"Name\", \"Icon\") VALUES ('Intersection', 'Path');";

        using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
        {
            targetConnection.Open(); // Open the connection

            using (NpgsqlCommand targetCommand = new NpgsqlCommand(insertQuery, targetConnection))
            {
                targetCommand.ExecuteNonQuery(); // Execute the insert command
            }

            targetConnection.Close(); // Close the connection
        }
    }

    private void DeleteDataFromTargetTable(string originTableName, string targetTableName, string targetConnectionString)
    {
        if (originTableName == "SpeedDevices")
        {
            return;
        }
        var deleteCommand = $"DELETE FROM {targetTableName}";
        using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
        {
            targetConnection.Open();

            using (NpgsqlCommand insertCommand = new NpgsqlCommand(deleteCommand, targetConnection))
            {

                Console.WriteLine($"Removing old records from {targetTableName}");
                insertCommand.CommandTimeout = 60;
                insertCommand.ExecuteNonQuery();
                Console.WriteLine($"Data deleted successfully.");
            }
        }
    }

    private void ExportToCsvAndImportToPostgres(SqlDataReader reader, string tableName, string postgresConnectionString, Dictionary<string, string> fieldMappings)
    {
        string tempFilePath = Path.GetTempFileName();

        using (StreamWriter writer = new StreamWriter(tempFilePath))
        {
            // Write CSV headers using new field names

            writer.WriteLine(string.Join("|", "\"" + fieldMappings.Values) + "\"");

            while (reader.Read())
            {
                var row = new List<string>();
                foreach (var mapping in fieldMappings)
                {
                    row.Add(reader[mapping.Key].ToString()); // Add necessary formatting
                }
                writer.WriteLine(string.Join("|", row));
            }
        }

        using (NpgsqlConnection conn = new NpgsqlConnection(postgresConnectionString))
        {

            var quotedFieldNames = fieldMappings.Values.Select(fieldName => $"\"{fieldName}\"");
            conn.Open();
            string copyCommand = $"COPY {tableName} ({string.Join(",", quotedFieldNames)}) FROM STDIN WITH CSV HEADER DELIMITER '|'";


            using (var writer = conn.BeginTextImport(copyCommand))
            using (var fileStream = File.OpenRead(tempFilePath))
            using (var streamReader = new StreamReader(fileStream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }
            }
        }

        File.Delete(tempFilePath);
    }

    private void ImportCsvToPostgres(string csvFilePath, string tableName, string targetConnectionString, Dictionary<string, string> fieldMappings)
    {
        using (NpgsqlConnection conn = new NpgsqlConnection(targetConnectionString))
        {
            var quotedFieldNames = fieldMappings.Values.Select(fieldName => $"\"{fieldName}\"");
            conn.Open();
            string copyCommand = $"COPY {tableName} ({string.Join(",", quotedFieldNames)}) FROM STDIN WITH CSV HEADER DELIMITER ','";

            using (var writer = conn.BeginTextImport(copyCommand))
            using (var fileStream = File.OpenRead(csvFilePath))
            using (var streamReader = new StreamReader(fileStream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }

}
