
using DatabaseInstaller.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

public class TransferConfigCommandHostedService : IHostedService
{
    private readonly ILogger<TransferConfigCommandHostedService> _logger;
    private readonly IJurisdictionRepository _jurisdictionRepository;
    private readonly ILocationTypeRepository _locationTypeRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IDeviceConfigurationRepository _deviceConfigurationRepository;
    private readonly TransferConfigCommandConfiguration _config;

    public TransferConfigCommandHostedService(
        ILogger<TransferConfigCommandHostedService> logger,
        IJurisdictionRepository jurisdictionRepository,
        ILocationTypeRepository locationTypeRepository,
        ILocationRepository locationRepository,
        IDeviceConfigurationRepository deviceConfigurationRepository,
        TransferConfigCommandConfiguration config
        )
    {
        _logger = logger;
        _jurisdictionRepository = jurisdictionRepository;
        _locationTypeRepository = locationTypeRepository;
        _locationRepository = locationRepository;
        _deviceConfigurationRepository = deviceConfigurationRepository;
        _config = config;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        if (_config.UpdateLocations)
        {
            if (_config.Delete)
            {
                DeleteLocations();
                DeleteDevices();
            }
        }
        //if (updateGeneralConfiguration)
        //{
        //    if (deleteOldData)
        //    {
        //        DeleteGeneralConfigurationData(targetConnectionString, jurisdictionIdsMapping);
        //    }
        //    ImportConfigurationData(targetConnectionString, sourceConnectionString, config);
        //    ManuallyAddJurisdiction();
        //}
        //if (updateLocations)
        //{
        //    ImportLocationsDataByJurisdiction(targetConnectionString, jurisdictionIdsMapping, sourceConnectionString, config);
        //}

        //ResetSequences(targetConnectionString);


        //Console.WriteLine("Data copied and bulk inserted.");
    }

    //private void ImportConfigurationData(string? targetConnectionString, string? sourceConnectionString, IConfiguration config)
    //{
    //    using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
    //    {
    //        sourceConnection.Open();
    //        Dictionary<string, string> queries = GetGeneralQueries(config);
    //        Dictionary<string, Dictionary<string, string>> columnMappings = GetColumnMappings();
    //        Dictionary<string, string> tableNameMappings = GetGeneralTableMappings();
    //        UpdateTables(targetConnectionString, sourceConnection, queries, columnMappings, tableNameMappings);
    //        //DeleteDataFromTargetTable("", "\"" + "DetectionTypeMeasureType" + "\"", targetConnectionString);
    //        ImportCsvToPostgres("./Files/DetectionTypeMeasureType.csv", "\"" + "DetectionTypeMeasureType" + "\"", targetConnectionString, columnMappings["DetectionTypeMetricTypes"]);
    //        ImportCsvToPostgres("./Files/LocationTypes.csv", "\"" + "LocationTypes" + "\"", targetConnectionString, columnMappings["LocationTypes"]);
    //        ImportCsvToPostgres("./Files/MeasureDefaults.csv", "\"" + "MeasureOptions" + "\"", targetConnectionString, columnMappings["MeasureOptions"]);
    //    }
    //}

    //private void ImportLocationsDataByJurisdiction(string? targetConnectionString, Dictionary<int, int> jurisdictionMap, string sourceConnectionString, IConfiguration config)
    //{
    //    using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
    //    {
    //        sourceConnection.Open();
    //        foreach (var jurisdiction in jurisdictionMap)
    //        {
    //            Dictionary<string, string> queries = GetLocationQueriesByJurisdictionId(config, Convert.ToInt32(jurisdiction.Key), Convert.ToInt32(jurisdiction.Value));
    //            Dictionary<string, Dictionary<string, string>> columnMappings = GetColumnMappings();
    //            Dictionary<string, string> tableNameMappings = GetLocationTableMappings();
    //            UpdateTables(targetConnectionString, sourceConnection, queries, columnMappings, tableNameMappings);
    //            //ManuallyAddPortlandConfiguration();
    //        }
    //    }
    //}

    //private void UpdateTables(string targetConnectionString, SqlConnection sourceConnection, Dictionary<string, string> queries, Dictionary<string, Dictionary<string, string>> columnMappings, Dictionary<string, string> tableNameMappings)
    //{
    //    try
    //    {
    //        //UpdateStaticData(targetConnectionString);

    //        foreach (var query in queries)
    //        {
    //            using (SqlCommand sourceCommand = new SqlCommand(query.Value, sourceConnection))
    //            using (SqlDataReader reader = sourceCommand.ExecuteReader())
    //            {
    //                string targetTableName = "\"" + tableNameMappings[query.Key] + "\"";
    //                ExportToCsvAndImportToPostgres(reader, targetTableName, targetConnectionString, columnMappings[query.Key]);
    //            }
    //            Console.WriteLine($"Data copied and bulk inserted for {query.Key}.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex.Message, "Error copying data from SQL Server to Postgres");
    //        Console.WriteLine(ex.Message);
    //    }
    //}

    //private static Dictionary<string, string> GetGeneralTableMappings()
    //{
    //    return new Dictionary<string, string>
    //    {
    //        {"Products", "Products" },
    //        {"DeviceConfigurations", "DeviceConfigurations" },
    //        {"Region", "Regions" },
    //        {"DetectionTypeMetricTypes", "DetectionTypeMeasureType" },
    //        {"Signals", "Devices"},
    //        {"Jurisdictions", "Jurisdictions"},
    //        {"ControllerTypes", "ControllerTypes"},
    //        {"Routes", "Routes"},
    //        {"Areas", "Areas"},
    //        {"AreaLocation", "AreaLocation"},
    //        {"RouteSignals", "RouteLocations"}
    //    };
    //}

    //private static Dictionary<string, string> GetLocationTableMappings()
    //{
    //    return new Dictionary<string, string>
    //    {
    //        {"Locations", "Locations"},
    //        {"Devices", "Devices"},
    //        {"SpeedDevices", "Devices"},
    //        {"Approaches", "Approaches"},
    //        {"Detectors", "Detectors"},
    //        {"DetectionTypeDetector", "DetectionTypeDetector"},
    //        {"AreaLocation", "AreaLocation"},
    //    };
    //}

    //private static Dictionary<string, Dictionary<string, string>> GetColumnMappings()
    //{
    //    // Define column mappings for each table
    //    return new Dictionary<string, Dictionary<string, string>>
    //{
    //    {
    //        "DeviceConfigurations", new Dictionary<string, string>
    //        {
    //            { "ControllerTypeID", "Id" },
    //            { "Description", "Firmware" },
    //            { "Protocol", "Protocol" },
    //            { "SNMPPort", "Port" },
    //            { "FTPDirectory", "Directory" },
    //            { "UserName", "UserName" },
    //            { "Password", "Password" },
    //            { "ProductId", "ProductId" }
    //        }
    //            },
    //    {
    //        "Products", new Dictionary<string, string>
    //        {
    //            { "ControllerTypeID", "Id" },
    //            { "Manufacturer", "Manufacturer" },
    //            { "Model", "Model" }
    //        }
    //            },
    //    {
    //        "Signals", new Dictionary<string, string>
    //        {
    //            { "SignalID", "LocationIdentifier" },
    //            { "VersionID", "Id" },
    //            { "Latitude", "Latitude" },
    //            { "Longitude", "Longitude" },
    //            { "PrimaryName", "PrimaryName" },
    //            { "SecondaryName", "SecondaryName" },
    //            { "IPAddress", "Ipaddress" },
    //            { "RegionID", "RegionId" },
    //            { "ControllerTypeID", "ControllerTypeId" },
    //            { "Enabled", "ChartEnabled" },
    //            { "LoggingEnabled", "LoggingEnabled" },
    //            { "VersionActionId", "VersionAction" },
    //            { "Note", "Note" },
    //            { "Start", "Start" },
    //            { "JurisdictionId", "JurisdictionId" },
    //            { "Pedsare1to1", "Pedsare1to1" }
    //        }
    //            },
    //    { "Locations", new Dictionary<string, string>
    //        {
    //            { "SignalID", "LocationIdentifier" },
    //            { "VersionID", "Id" },
    //            { "Latitude", "Latitude" },
    //            { "Longitude", "Longitude" },
    //            { "PrimaryName", "PrimaryName" },
    //            { "SecondaryName", "SecondaryName" },
    //            { "RegionId", "RegionId" },
    //            { "Enabled", "ChartEnabled" },
    //            { "VersionActionId", "VersionAction" },
    //            { "Start", "Start" },
    //            { "JurisdictionId", "JurisdictionId" },
    //            { "PedsAre1to1", "PedsAre1to1" },
    //            { "LocationTypeId", "LocationTypeId" }
    //        }
    //            },
    //    { "Devices", new Dictionary<string, string>
    //        {
    //            { "LoggingEnabled", "LoggingEnabled" },
    //            { "Status", "DeviceStatus" },
    //            { "IPAddress", "Ipaddress" },
    //            { "ControllerTypeID", "DeviceConfigurationId" },
    //            { "LocationId", "LocationId" },
    //            { "Note", "Notes" },
    //            { "DeviceType", "DeviceType" }
    //        }
    //            },
    //    { "SpeedDevices", new Dictionary<string, string>
    //        {
    //            { "LoggingEnabled", "LoggingEnabled" },
    //            { "Status", "DeviceStatus" },
    //            { "IPAddress", "Ipaddress" },
    //            { "ControllerTypeID", "DeviceConfigurationId" },
    //            { "LocationId", "LocationId" },
    //            { "Note", "Notes" },
    //            { "DeviceType", "DeviceType" }
    //        }
    //            },
    //    {
    //        "ControllerTypes", new Dictionary<string, string>
    //        {
    //            { "ControllerTypeID", "Id" },
    //            { "Description", "Product" },
    //            { "SNMPPort", "Port" },
    //            { "FTPDirectory", "Directory" },
    //            { "UserName", "UserName" },
    //            { "Password", "Password" },
    //        }
    //    },
    //    {
    //        "Approaches", new Dictionary<string, string>
    //        {
    //            { "ApproachID", "Id" },
    //            { "VersionId", "LocationId" },
    //            { "DirectionTypeID", "DirectionTypeId" },
    //            { "Description", "Description" },
    //            { "MPH", "Mph" },
    //            { "ProtectedPhaseNumber", "ProtectedPhaseNumber" },
    //            { "IsProtectedPhaseOverlap", "IsProtectedPhaseOverlap" },
    //            { "PermissivePhaseNumber", "PermissivePhaseNumber" },
    //            { "IsPermissivePhaseOverlap", "IsPermissivePhaseOverlap" },
    //            { "PedestrianPhaseNumber", "PedestrianPhaseNumber" },
    //            { "IsPedestrianPhaseOverlap", "IsPedestrianPhaseOverlap" },
    //            { "PedestrianDetectors", "PedestrianDetectors" }
    //        }
    //    },
    //    {
    //        "Jurisdictions", new Dictionary<string, string>
    //        {
    //            { "Id", "Id" },
    //            { "JurisdictionName", "Name" },
    //            { "Mpo", "Mpo" },
    //            { "CountyParish", "CountyParish" },
    //            { "OtherPartners", "OtherPartners" }
    //        }
    //    },
    //    {
    //        "Region", new Dictionary<string, string>
    //        {
    //            { "ID", "Id" },
    //            { "Description", "Description" }
    //        }
    //    },
    //    {
    //        "Areas", new Dictionary<string, string>
    //        {
    //            { "Id", "Id" },
    //            { "AreaName", "Name" }
    //        }
    //    },
    //    {
    //        "AreaLocation", new Dictionary<string, string>
    //        {
    //            { "Area_Id", "AreasId" },
    //            { "Signal_VersionID", "LocationsId" }
    //        }
    //    },
    //    {
    //        "Detectors", new Dictionary<string, string>
    //        {
    //                { "ID", "Id"},
    //                {"DetectorID", "DectectorIdentifier"},
    //                {"DetChannel", "DetectorChannel"},
    //                {"DistanceFromStopBar", "DistanceFromStopBar"},
    //                {"MinSpeedFilter", "MinSpeedFilter"},
    //                {"DateAdded", "DateAdded"},
    //                {"DateDisabled", "DateDisabled"},
    //                {"LaneNumber", "LaneNumber"},
    //                {"MovementTypeID", "MovementType"},
    //                {"LaneTypeId", "LaneType"},
    //                {"DecisionPoint", "DecisionPoint"},
    //                {"MovementDelay", "MovementDelay"},
    //                {"ApproachID", "ApproachId"},
    //                {"DetectionHardwareID", "DetectionHardware"},
    //                { "LatencyCorrection", "LatencyCorrection"}
    //        }
    //    },
    //    {
    //        "DetectionTypeMetricTypes", new Dictionary<string, string>
    //        {
    //            {"DetectionType_DetectionTypeID", "DetectionTypesId"},
    //            {"MetricType_MetricID", "MeasureTypesId"}
    //        }
    //    },
    //    {
    //        "DetectionTypeDetector",  new Dictionary<string, string>
    //        {
    //            {"DetectionTypeId", "DetectionTypesId"},
    //            {"ID", "DetectorsId"}
    //        }
    //    },
    //    {
    //        "Routes",  new Dictionary<string, string>
    //        {
    //            {"Id", "Id"},
    //            {"RouteName", "Name"}
    //        }
    //    },
    //    {
    //        "RouteSignals",  new Dictionary<string, string>
    //        {
    //            {"Id", "Id"},
    //            {"RouteId", "RouteId"},
    //            {"Order", "Order"},
    //            {"SignalId", "LocationIdentifier"},
    //            {"PrimaryPhase", "PrimaryPhase"},
    //            {"OpposingPhase", "OpposingPhase"},
    //            {"PrimaryDirectionId", "PrimaryDirectionId"},
    //            {"OpposingDirectionId", "OpposingDirectionId"},
    //            {"IsPrimaryOverlap", "IsPrimaryOverlap"},
    //            {"IsOpposingOverlap", "IsOpposingOverlap"}
    //        }
    //    },
    //    {
    //        "LocationTypes",  new Dictionary<string, string>
    //        {
    //            {"Id", "Id"},
    //            {"Name", "Name"},
    //            {"Icon", "Icon"},
    //        }
    //    },
    //    {
    //        "MeasureOptions",  new Dictionary<string, string>
    //        {
    //            {"Id", "Id"},
    //            {"Option", "Option"},
    //            {"Value", "Value"},
    //            {"MeasureTypeId", "MeasureTypeId"},
    //        }
    //    }
    //};
    //}

    //private Dictionary<string, string> GetGeneralQueries(IConfiguration config)
    //{
    //    var detectionTypeMetricTypesQuery = config.GetSection("DetectionTypeMetricTypesQuery").Get<string>();

    //    var queries = new Dictionary<string, string>
    //    {
    //        { "Jurisdictions", "SELECT [Id], [JurisdictionName], [Mpo], [CountyParish], [OtherPartners] FROM Jurisdictions" },
    //        { "Region", "Select [ID], [Description] FROM Region" },
    //        { "Areas", "SELECT Id, AreaName FROM Areas" },
    //        { "Products", "SELECT [ControllerTypeID], CASE WHEN [Description] LIKE 'ASC3%' THEN 'Econolite' WHEN [Description] = 'Cobalt' THEN 'QFree' WHEN [Description] = 'MaxTime' THEN 'QFree'  WHEN [Description] = 'Trafficware' THEN 'Trafficware'  WHEN [Description] = 'Siemens SEPAC' THEN 'Siemens'  WHEN [Description] = 'McCain ATC EX' THEN 'McCain'  WHEN [Description] = 'Peek' THEN 'Peek'  WHEN [Description] = 'EOS' THEN 'Econolite' ELSE [Description] END AS Manufacturer, CASE WHEN [Description] LIKE 'ASC3 - 2070' THEN '2070'  WHEN [Description] = 'Siemens SEPAC' THEN 'SEPAC'  WHEN [Description] = 'McCain ATC EX' THEN 'ATC EX' WHEN [Description] = 'ASC3-32.68.40' THEN '32.68.40' ELSE [Description] END AS Model, [Description] FROM [MOE].[dbo].[ControllerTypes];" },
    //        { "DeviceConfigurations", "SELECT [ControllerTypeID], [ControllerTypeID] as ProductId, [Description], 'SFTP' as Protocol, SNMPPort, FTPDirectory, [UserName], [Password]  FROM [MOE].[dbo].[ControllerTypes]" },
    //        //{ "DetectionTypeMetricTypes", detectionTypeMetricTypesQuery }
    //    };

    //    foreach (var key in queries.Keys)
    //    {
    //        _logger.LogInformation($"Query for {key} is {queries[key]}");
    //    }

    //    return queries;
    //}

    //private Dictionary<string, string> GetLocationQueriesByJurisdictionId(IConfiguration config, int jurisdictionId, int targetJurisdictionId)
    //{
    //    var idRange = config.GetSection("IdRange").Get<string>();
    //    var prependIdentifier = config.GetSection("PrependIdentifier").Get<string>();
    //    var queries = new Dictionary<string, string>
    //    {
    //        { "Locations", config.GetSection("LocationQuery").Get<string>()??string.Empty },
    //        { "SpeedDevices", config.GetSection("SpeedDevicesQuery").Get<string>()??string.Empty  },
    //        { "Devices", config.GetSection("DevicesQuery").Get<string>() ?? string.Empty },
    //        { "Approaches", config.GetSection("ApproachesQuery").Get<string>() ?? string.Empty },
    //        { "Detectors", config.GetSection("DetectorsQuery").Get<string>()??string.Empty  },
    //        { "DetectionTypeDetector", config.GetSection("DetectionTypeDetectorQuery").Get<string>() ?? string.Empty },
    //        { "AreaLocation", config.GetSection("AreaSignalsQuery").Get<string>() ?? string.Empty }
    //    };
    //    foreach (var key in queries.Keys)
    //    {
    //        queries[key] = queries[key]
    //            .Replace("{jurisdictionId}", jurisdictionId.ToString())
    //            .Replace("{IdRange}", idRange.ToString())
    //            .Replace("{PrependIdentifier}", prependIdentifier.ToString())
    //            .Replace("{TargetJurisdictionId}", targetJurisdictionId.ToString());
    //        _logger.LogInformation($"Query for {key} is {queries[key]}");
    //    }
    //    return queries;
    //}

    //private static Dictionary<string, string> GetRouteQueries(int jurisdictionId, IConfiguration _config)
    //{
    //    var queries = new Dictionary<string, string>();
    //    queries.Add("Routes", "SELECT [Id] ,[RouteName] FROM [MOE].[dbo].[Routes]");
    //    queries.Add("RouteSignals", "SELECT r.[Id]\r\n      ,[RouteId]\r\n      ,[Order]\r\n      ,[LocationIdentifier]\r\n\t  ,(SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) as PrimaryPhase\r\n  ,(SELECT DirectionTypeId\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) as PrimaryDirectionId\r\n   ,(SELECT IsOverlap\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) as IsPrimaryOverlap\r\n  ,(SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) as OpposingPhase\r\n  ,(SELECT DirectionTypeId\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) as OpposingDirectionId\r\n  ,(SELECT IsOverlap\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) as IsOpposingOverlap\r\n  FROM [MOE].[dbo].[RouteSignals] r\r\n  Where (SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 1\r\n  and RouteSignalId = r.Id) Is Not NULL And (SELECT [Phase]\r\n  FROM [MOE].[dbo].[RoutePhaseDirections]\r\n  where IsPrimaryApproach = 0\r\n  and RouteSignalId = r.Id) Is Not NULL");
    //    return queries;
    //}

    //private void ResetSequences(string targetConnectionString)
    //{
    //    string[] sequences = new string[]
    //    {
    //    "\"Locations_Id_seq\"",
    //    "\"Jurisdictions_Id_seq\"",
    //    "\"Approaches_Id_seq\"",
    //    "\"Detectors_Id_seq\"",
    //    "\"Products_Id_seq\"",
    //    "\"DeviceConfigurations_Id_seq\"",
    //    "\"Regions_Id_seq\"",
    //    "\"Devices_Id_seq\"",
    //    "\"Routes_Id_seq\"",
    //    "\"Areas_Id_seq\"",
    //    "\"MenuItems_Id_seq\"",
    //    "\"RouteLocations_Id_seq\""
    //    };

    //    using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
    //    {
    //        targetConnection.Open();
    //        foreach (string sequence in sequences)
    //        {
    //            // Correctly format table name derived from sequence name
    //            string tableName = sequence.Replace("_Id_seq\"", "").Replace("\"", "");

    //            string query = $"SELECT setval('public.{sequence}', (SELECT COALESCE(MAX(\"Id\"), 0) FROM public.\"{tableName}\") + 1);";

    //            using (NpgsqlCommand cmd = new NpgsqlCommand(query, targetConnection))
    //            {
    //                cmd.ExecuteNonQuery();
    //                Console.WriteLine($"Sequence {sequence} reset successfully.");
    //            }
    //        }
    //    }
    //}


    //private void AddLocationTypes()
    //{
    //    if (_locationTypeRepository.GetList().Any())
    //    {
    //        return;
    //    }
    //    _locationTypeRepository.Add(new LocationType { Name = "Intersection" });
    //}

    //private void ManuallyAddJurisdiction()
    //{
    //    var jurisdictionIdList = new List<int> { 47, 48 };
    //    if (!_jurisdictionRepository.GetList().Where(j => j.Id == 47).Any())
    //    {
    //        _jurisdictionRepository.Add(new Jurisdiction { Id = 47, Name = "City of Portland", Mpo = "PBOT" });
    //    }
    //    if (!_jurisdictionRepository.GetList().Where(j => j.Id == 48).Any())
    //    {
    //        _jurisdictionRepository.Add(new Jurisdiction { Id = 48, Name = "Albuquerque" });
    //    }
    //}

    //private void ManuallyAddSpeedDevice()
    //{
    //    productRepository.Add(new Product { Id = 11, Manufacturer = "Wavetronix", Model = "Wavetronix Advance Detection" });
    //    deviceConfigurationRepository.Add(new DeviceConfiguration
    //    {
    //        Id = 11,
    //        Firmware = "None",
    //        Protocol = ATSPM.Table.Enums.TransportProtocols.Unknown,
    //        ConnectionTimeout = 2000,
    //        Directory = "Unkown",
    //        OperationTimout = 2000,
    //        Port = 0,
    //        UserName = "Unknown",
    //        Password = "Unkown",
    //        ProductId = 11
    //    });
    //    deviceRepository.Add(new Device
    //    {
    //         DeviceConfigurationId= 11, DeviceStatus = ATSPM.Table.Enums.DeviceStatus.Active, Ipaddress = "127.0.0.1", 
    //    });
    //}

    //private void UpdateStaticData(string targetConnectionString)
    //{
    //    string insertQuery = "INSERT INTO public.\"LocationTypes\"(\"Name\", \"Icon\") VALUES ('Intersection', 'Path');";

    //    using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
    //    {
    //        targetConnection.Open(); // Open the connection

    //        using (NpgsqlCommand targetCommand = new NpgsqlCommand(insertQuery, targetConnection))
    //        {
    //            targetCommand.ExecuteNonQuery(); // Execute the insert command
    //        }

    //        targetConnection.Close(); // Close the connection
    //    }
    //}

    private void DeleteLocations()
    {
        _logger.LogInformation($"Deleting all locations");
        try
        {
            var locations = _locationRepository.GetList().ToList();
            _locationRepository.RemoveRange(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting locations");
            throw;
        }
    }

    private void DeleteDevices()
    {
        _logger.LogInformation($"Deleting all devices");
        try
        {
            _deviceConfigurationRepository.RemoveRangeAsync(_deviceConfigurationRepository.GetList().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error deleting devices");
            throw;
        }
    }

    //private void DeleteGeneralConfigurationData(string targetConnectionString, Dictionary<int, int> jurisdictionIdsMapping)
    //{
    //    //get a comma seperated list of jurisdiction ids
    //    string jurisdictionIds = string.Join(",", jurisdictionIdsMapping.Values);
    //    var deleteCommands = new List<string>()
    //    {
    //        $"DELETE FROM public.\"Jurisdictions\" Where \"Id\" in ({jurisdictionIds})",
    //        $"DELETE FROM public.\"Regions\"",
    //        $"DELETE FROM public.\"Areas\"",
    //        $"DELETE FROM public.\"DeviceConfigurations\"",
    //        $"DELETE FROM public.\"Products\"",
    //        $"DELETE FROM public.\"DetectionTypeMeasureType\"",
    //        $"DELETE FROM public.\"LocationTypes\"",
    //        $"DELETE FROM public.\"MeasureOptions\""
    //    };
    //    using (NpgsqlConnection targetConnection = new NpgsqlConnection(targetConnectionString))
    //    {
    //        targetConnection.Open();
    //        foreach (string deleteCommand in deleteCommands)
    //        {
    //            using (NpgsqlCommand insertCommand = new NpgsqlCommand(deleteCommand, targetConnection))
    //            {
    //                Console.WriteLine(deleteCommand);
    //                insertCommand.CommandTimeout = 60;
    //                insertCommand.ExecuteNonQuery();
    //                Console.WriteLine($"Data deleted successfully.");
    //            }
    //        }
    //    }
    //}

    //private void ExportToCsvAndImportToPostgres(SqlDataReader reader, string tableName, string postgresConnectionString, Dictionary<string, string> fieldMappings)
    //{
    //    string tempFilePath = Path.GetTempFileName();

    //    using (StreamWriter writer = new StreamWriter(tempFilePath))
    //    {
    //        // Write CSV headers using new field names
    //        writer.WriteLine(string.Join("|", fieldMappings.Values.Select(v => $"\"{v}\"")));

    //        while (reader.Read())
    //        {
    //            var row = new List<string>();
    //            foreach (var mapping in fieldMappings)
    //            {
    //                row.Add(reader[mapping.Key].ToString()); // Add necessary formatting
    //            }
    //            writer.WriteLine(string.Join("|", row));
    //        }
    //    }

    //    using (NpgsqlConnection conn = new NpgsqlConnection(postgresConnectionString))
    //    {

    //        var quotedFieldNames = fieldMappings.Values.Select(fieldName => $"\"{fieldName}\"");
    //        conn.Open();
    //        string copyCommand = $"COPY {tableName} ({string.Join(",", quotedFieldNames)}) FROM STDIN WITH CSV HEADER DELIMITER '|'";


    //        using (var writer = conn.BeginTextImport(copyCommand))
    //        using (var fileStream = File.OpenRead(tempFilePath))
    //        using (var streamReader = new StreamReader(fileStream))
    //        {
    //            string line;
    //            while ((line = streamReader.ReadLine()) != null)
    //            {
    //                writer.WriteLine(line);
    //            }
    //        }
    //    }

    //    File.Delete(tempFilePath);
    //}

    //private void ImportCsvToPostgres(string csvFilePath, string tableName, string targetConnectionString, Dictionary<string, string> fieldMappings)
    //{
    //    using (NpgsqlConnection conn = new NpgsqlConnection(targetConnectionString))
    //    {
    //        var quotedFieldNames = fieldMappings.Values.Select(fieldName => $"\"{fieldName}\"");
    //        conn.Open();
    //        string copyCommand = $"COPY {tableName} ({string.Join(",", quotedFieldNames)}) FROM STDIN WITH CSV HEADER DELIMITER ','";

    //        using (var writer = conn.BeginTextImport(copyCommand))
    //        using (var fileStream = File.OpenRead(csvFilePath))
    //        using (var streamReader = new StreamReader(fileStream))
    //        {
    //            string line;
    //            while ((line = streamReader.ReadLine()) != null)
    //            {
    //                writer.WriteLine(line);
    //            }
    //        }
    //    }
    //}

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
