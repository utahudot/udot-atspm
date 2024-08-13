using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;
using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;

public class CopyConfigurationFromCsv : Command
{
    private readonly ILogger<CopyConfigurationNewPostgres> logger;
    private readonly ILocationRepository locationRepository;
    private readonly IDetectionTypeRepository detectionTypeRepository;

    public CopyConfigurationFromCsv(ILogger<CopyConfigurationNewPostgres> logger, ILocationRepository locationRepository, IDetectionTypeRepository detectionTypeRepository) : base("copy-config-csv", "Copy Config Data for new data format in a posgres database")
    {
        Handler = CommandHandler.Create(Execute);
        this.logger = logger;
        this.locationRepository = locationRepository;
        this.detectionTypeRepository = detectionTypeRepository;
    }

    private void Execute()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        string targetConnectionString = config.GetConnectionString("TargetConfigDBConnectionString");

        string basePath = Path.Combine(Directory.GetCurrentDirectory(), "Files");
        string signalsFilePath = Path.Combine(basePath, "Signals.csv");
        string approachesFilePath = Path.Combine(basePath, "Approaches.csv");
        string detectorsFilePath = Path.Combine(basePath, "Detectors.csv");
        string detectionTypeDetectorFilePath = Path.Combine(basePath, "DetectionTypeDetector.csv");



        Dictionary<int, Location> locations = new Dictionary<int, Location>();
        using (TextFieldParser parser = new TextFieldParser(signalsFilePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();

                var location = new Location
                {
                    LocationIdentifier = $"p-{fields[0]}",
                    Latitude = double.Parse(fields[1]),
                    Longitude = double.Parse(fields[2]),
                    PrimaryName = fields[3],
                    SecondaryName = fields[4],
                    ChartEnabled = true,
                    VersionAction = (LocationVersionActions)int.Parse(fields[12]),
                    Start = DateTime.Parse(fields[14]),
                    PedsAre1to1 = true,
                    LocationTypeId = 1,
                    JurisdictionId = 47,
                    RegionId = 10,
                };
                var device = new Device
                {
                    DeviceConfigurationId = int.Parse(fields[7]),
                    DeviceStatus = DeviceStatus.Active,
                    DeviceType = DeviceTypes.SignalController,
                    Ipaddress = "127.0.0.1",
                    LoggingEnabled = true
                };
                location.Devices.Add(device);
                locations.Add(int.Parse(fields[11]), location);
            }
        }

        var approachesDictionary = new Dictionary<int, Approach>();
        using (TextFieldParser parser = new TextFieldParser(approachesFilePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                var locationId = int.Parse(fields[8]);
                var locationIdentifier = $"p-{fields[1]}";
                var location = locations[locationId];

                var approach = new Approach
                {
                    DirectionTypeId = (DirectionTypes)int.Parse(fields[2]),
                    Description = fields[3],
                    Mph = fields[4] == "NULL" ? null : int.Parse(fields[4]),
                    ProtectedPhaseNumber = int.Parse(fields[5]),
                    IsProtectedPhaseOverlap = fields[6] == "0" ? false : true,
                    PermissivePhaseNumber = fields[7] == "NULL" ? null : int.Parse(fields[7]),
                    IsPermissivePhaseOverlap = fields[9] == "0" ? false : true,
                };
                location.Approaches.Add(approach);
                approachesDictionary.Add(int.Parse(fields[0]), approach);
            }
        }

        var detectionTypesDictionary = new List<Tuple<int, int>>();
        using (TextFieldParser parser = new TextFieldParser(detectionTypeDetectorFilePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                detectionTypesDictionary.Add(new Tuple<int, int>(int.Parse(fields[0]), int.Parse(fields[1])));
            }
        }

        var detectionTypes = detectionTypeRepository.GetList().ToList();
        using (TextFieldParser parser = new TextFieldParser(detectorsFilePath))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields();
                var approachId = int.Parse(fields[12]);
                var approach = approachesDictionary[approachId];
                var detectionTypeIds = detectionTypesDictionary.Where(dt => dt.Item1 == int.Parse(fields[0])).Select(dt => dt.Item2).ToList();
                var detectionTypesForDetector = detectionTypes.Where(dt => detectionTypeIds.Contains((int)dt.Id)).ToList();

                approach.Detectors.Add(new Detector
                {
                    DectectorIdentifier = fields[1],
                    DetectorChannel = int.Parse(fields[2]),
                    DistanceFromStopBar = fields[3] == "NULL" ? null : int.Parse(fields[3]),
                    MinSpeedFilter = fields[4] == "NULL" ? null : int.Parse(fields[4]),
                    DateAdded = DateTime.Parse(fields[5]),
                    DateDisabled = fields[6] == "NULL" ? null : DateTime.Parse(fields[6]),
                    LaneNumber = fields[7] == "NULL" ? null : int.Parse(fields[7]),
                    MovementType = fields[8] == "NULL" ? MovementTypes.NA : (MovementTypes)int.Parse(fields[8]),
                    LaneType = fields[9] == "NULL" ? LaneTypes.NA : (LaneTypes)int.Parse(fields[9]),
                    DecisionPoint = fields[10] == "NULL" ? null : int.Parse(fields[10]),
                    MovementDelay = fields[11] == "NULL" ? null : int.Parse(fields[11]),
                    DetectionHardware = (DetectionHardwareTypes)int.Parse(fields[13]),
                    LatencyCorrection = int.Parse(fields[14]),
                    DetectionTypes = detectionTypesForDetector,
                });
            }
        }

        foreach (var location in locations)
        {
            locationRepository.Add(location.Value);
        }

        //using (var workbook = new XLWorkbook(filePath))
        //{
        //    foreach (var worksheet in workbook.Worksheets)
        //    {
        //        if (worksheet.Name == "route")
        //        {
        //            continue;
        //        }
        //        var range = worksheet.RangeUsed();
        //        var locationIdentifier = $"p-{worksheet.Name}";
        //        var location = new Location
        //        {
        //            LocationIdentifier = locationIdentifier, 
        //            PrimaryName = locationIdentifier, 
        //            SecondaryName = locationIdentifier, ChartEnabled = true, PedsAre1to1 = false, Start
        //            Latitude = 0,
        //            Longitude = 0,
        //            LocationTypeId = 1,
        //            JurisdictionId = 49,
        //            RegionId = 10
        //        };

        //        List<PortlandCsv> portlandCsvs = new List<PortlandCsv>();
        //        for (int row = 2; row <= range.RowCount(); row++) // Assuming row 1 has headers
        //        {
        //            portlandCsvs.Add(new PortlandCsv
        //            {
        //                DetectorID = worksheet.Cell(row, 1).GetValue<string>(),
        //                DetChannel = worksheet.Cell(row, 2).GetValue<int>(),
        //                Direction = worksheet.Cell(row, 3).GetValue<string>(),
        //                Phase = worksheet.Cell(row, 4).GetValue<int>(),
        //                PermPhase = worksheet.Cell(row, 5).GetValue<int?>(),
        //                Overlap = worksheet.Cell(row, 6).GetValue<bool>(),
        //                Enabled = worksheet.Cell(row, 7).GetValue<bool>(),
        //                DetectionTypes = worksheet.Cell(row, 8).GetValue<string>(),
        //                DetectionHardware = worksheet.Cell(row, 9).GetValue<string>(),
        //                LatencyCorrection = worksheet.Cell(row, 10).GetValue<int>(),
        //                MovementType = worksheet.Cell(row, 11).GetValue<string>(),
        //                LaneNumber = worksheet.Cell(row, 12).GetValue<int>(),
        //                LaneType = worksheet.Cell(row, 13).GetValue<string>(),
        //                MPH = worksheet.Cell(row, 14).GetValue<int?>(),
        //                DistFromStopBar = worksheet.Cell(row, 15).GetValue<int?>(),
        //                DecisionPoint = worksheet.Cell(row, 16).GetValue<int?>(),
        //                MoveDelay = worksheet.Cell(row, 17).GetValue<int?>(),
        //                MinSpeedFilter = worksheet.Cell(row, 18).GetValue<int?>(),
        //                Comment = worksheet.Cell(row, 19).GetValue<string>()
        //            });

        //            Console.WriteLine($"Location ID: {locationIdentifier}");
        //            // Process other columns as necessary
        //        }
        //        var phases = portlandCsvs.Select(p => new { p.Phase, p.Direction, p.MPH, p.Overlap }).Distinct();
        //        //Create an approach for phase in phases dictionary
        //        var approaches = new List<Approach>();
        //        foreach (var phase in phases)
        //        {
        //            var approach = new Approach
        //            {
        //                DirectionTypeId = GetDirectionType(phase.Direction),
        //                ProtectedPhaseNumber = phase.Phase,
        //                Mph = phase.MPH,
        //                IsProtectedPhaseOverlap = phase.Overlap,
        //                Description = $"{phase.Direction} - {phase.Phase}"
        //            };
        //            foreach (var row in portlandCsvs.Where(p => p.Phase == phase.Phase && p.Direction == phase.Direction))
        //            {
        //                approach.Detectors.Add(new Detector
        //                {
        //                    DectectorIdentifier = row.DetectorID,
        //                    MovementType = GetMovementType(row.MovementType),
        //                    LaneNumber = row.LaneNumber,
        //                    LaneType = GetLaneType(row.LaneType),
        //                    DistanceFromStopBar = row.DistFromStopBar,
        //                    DecisionPoint = row.DecisionPoint,
        //                    MovementDelay = row.MoveDelay,
        //                    MinSpeedFilter = row.MinSpeedFilter,
        //                    DetectorComments = new List<DetectorComment> { new DetectorComment { Comment = row.Comment, TimeStamp = DateTime.Now } }
        //                });
        //            }
        //            location.Approaches.Add(approach);
        //        }
        //        locationRepository.Add(location);
        //    }

        //}


        //try
        //{
        //    //UpdateStaticData(targetConnectionString);

        //    foreach (var query in queries)
        //    {
        //        using (SqlCommand sourceCommand = new SqlCommand(query.Value, sourceConnection))
        //        using (SqlDataReader reader = sourceCommand.ExecuteReader())
        //        {
        //            string targetTableName = "\"" + tableNameMappings[query.Key] + "\"";
        //            DeleteDataFromTargetTable(query.Key, targetTableName, targetConnectionString);
        //            ExportToCsvAndImportToPostgres(reader, targetTableName, targetConnectionString, columnMappings[query.Key]);
        //        }
        //        Console.WriteLine($"Table copied and bulk inserted for {query.Key}.");
        //    }
        //    DeleteDataFromTargetTable("", "\"" + "DetectionTypeMeasureType" + "\"", targetConnectionString);
        //    ImportCsvToPostgres("./Files/DetectionTypeMeasureType.csv", "\"" + "DetectionTypeMeasureType" + "\"", targetConnectionString, columnMappings["DetectionTypeMetricTypes"]);
        //}
        //catch (Exception ex)
        //{
        //    logger.LogError(ex.Message, "Error copying data from SQL Server to Postgres");
        //    Console.WriteLine(ex.Message);
        //}


        Console.WriteLine("Data copied and bulk inserted.");
    }

    private LaneTypes GetLaneType(string laneType)
    {
        switch (laneType)
        {
            case "Vehicle":
                return LaneTypes.V;
            case "Bus":
                return LaneTypes.Bus;
            case "Bike":
                return LaneTypes.Bike;
            default:
                return LaneTypes.NA;
        }
    }

    private MovementTypes GetMovementType(string movementType)
    {
        switch (movementType)
        {
            case "Left":
                return MovementTypes.L;
            case "Thru":
                return MovementTypes.T;
            case "Right":
                return MovementTypes.R;
            case "Thru-Left":
                return MovementTypes.TL;
            case "Thru-Right":
                return MovementTypes.TR;
            default:
                return MovementTypes.NA;
        }
    }

    private DirectionTypes GetDirectionType(string value)
    {
        switch (value)
        {
            case "NB":
                return DirectionTypes.NB;
            case "SB":
                return DirectionTypes.SB;
            case "EB":
                return DirectionTypes.EB;
            case "WB":
                return DirectionTypes.WB;
            default:
                return DirectionTypes.NB;
        }
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
