using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Text;

public class CreateAggregationsForTestSite : Command
{
    private readonly ILogger<CreateAggregationsForTestSite> logger;
    private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
    private readonly IServiceProvider serviceProvider;

    public CreateAggregationsForTestSite(ILogger<CreateAggregationsForTestSite> logger, IPhaseCycleAggregationRepository phaseCycleRepository, IServiceProvider serviceProvider) : base("create-aggregations", "Create Aggregations command")
    {
        var dateOption = new Option<DateTime?>("--date", "The date");
        dateOption.IsRequired = false; // Make the option not required
        AddOption(dateOption);

        var locationOption = new Option<string?>("--location", "Location");
        locationOption.IsRequired = false; // Make the option not required
        AddOption(locationOption);

        Handler = CommandHandler.Create<DateTime?, string?>(ExecuteAsync);
        this.logger = logger;
        this.phaseCycleAggregationRepository = phaseCycleRepository;
        this.serviceProvider = serviceProvider;
    }

    private async Task ExecuteAsync(DateTime? date, string? location)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string sourceConnectionString = config.GetConnectionString("SourceConnectionString");
        string targetConnectionString = config.GetConnectionString("TargetAggregationDBConnectionString");
        string targetConfigConnectionString = config.GetConnectionString("TargetConfigDBConnectionString");
        Console.WriteLine($"Source is {sourceConnectionString}");
        Console.WriteLine($"Target is {targetConnectionString}");
        //Console.WriteLine($"Press any key to continue or close the window");
        //Console.ReadKey();

        var signalsQuery = "SELECT distinct [SignalID] FROM [MOE].[dbo].[Signals] where VersionActionId != 3";
        if (location != null)
        {
            signalsQuery += $" and SignalID in ('{location}')";
        }
        signalsQuery += " order by [SignalID]";
        var dateToRetrieve = date == null ? DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) : DateOnly.FromDateTime(date.Value);

        var locationDevicesDict = new Dictionary<string, int>();

        try
        {
            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
            {
                sourceConnection.Open();
                var signals = new List<string>();
                using (SqlCommand selectCommand = new SqlCommand(signalsQuery, sourceConnection))
                {
                    selectCommand.CommandTimeout = 120;
                    Console.WriteLine($"Executing query: {signalsQuery}");
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            signals.Add(reader["SignalID"].ToString());
                        }
                    }
                }
                var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
                //Parallel.ForEach(signals, options, signalId =>
                var cycleAggregations = new List<CompressedAggregations<PhaseCycleAggregation>>();
                var detectorAggregations = new List<CompressedAggregations<DetectorEventCountAggregation>>();
                var terminationAggregations = new List<CompressedAggregations<PhaseTerminationAggregation>>();
                var splitFailAggregations = new List<CompressedAggregations<ApproachSplitFailAggregation>>();
                var leftTurnAggregations = new List<CompressedAggregations<PhaseLeftTurnGapAggregation>>();
                var pedAggregations = new List<CompressedAggregations<PhasePedAggregation>>();
                var pcdAggregations = new List<CompressedAggregations<ApproachPcdAggregation>>();
                var speedAggregations = new List<CompressedAggregations<ApproachSpeedAggregation>>();
                var yellowRedAggregations = new List<CompressedAggregations<ApproachYellowRedActivationAggregation>>();
                var preemptAggregations = new List<CompressedAggregations<PreemptionAggregation>>();
                var priorityAggregations = new List<CompressedAggregations<PriorityAggregation>>();
                var signalEventCountAggregations = new List<CompressedAggregations<SignalEventCountAggregation>>();
                var splitMonitorAggregations = new List<CompressedAggregations<PhaseSplitMonitorAggregation>>();
                foreach (var signalId in signals)
                {
                    //if (!locationDevicesDict.ContainsKey(signalId))
                    //{
                    //    Console.WriteLine($"No device found for signal {signalId}");
                    //    continue;
                    //}
                    GetPhaseCycleAggregations(dateToRetrieve, sourceConnection, cycleAggregations, signalId);
                    GetDetectorEventCountAggregations(dateToRetrieve, sourceConnection, detectorAggregations, signalId);
                    GetPhaseTerminationAggregationAggregations(dateToRetrieve, sourceConnection, terminationAggregations, signalId);
                    GetSplitFailAggregations(dateToRetrieve, sourceConnection, splitFailAggregations, signalId);
                    GetLeftTurnAggregations(dateToRetrieve, sourceConnection, leftTurnAggregations, signalId);
                    GetPedAggregations(dateToRetrieve, sourceConnection, pedAggregations, signalId);
                    GetPCDAggregations(dateToRetrieve, sourceConnection, pcdAggregations, signalId);
                    GetSpeedAggregations(dateToRetrieve, sourceConnection, speedAggregations, signalId);
                    GetYellowRedAggregations(dateToRetrieve, sourceConnection, yellowRedAggregations, signalId);
                    GetPreemptAggregations(dateToRetrieve, sourceConnection, preemptAggregations, signalId);
                    GetPriorityAggregations(dateToRetrieve, sourceConnection, priorityAggregations, signalId);
                    GetSignalEventAggregations(dateToRetrieve, sourceConnection, signalEventCountAggregations, signalId);
                    GetSplitMonitorAggregations(dateToRetrieve, sourceConnection, splitMonitorAggregations, signalId);
                    //if (terminationAggregations.Count > 10)
                    //{

                    SaveAggregationsToDatabase(
                        cycleAggregations,
                        detectorAggregations,
                        splitFailAggregations,
                        leftTurnAggregations,
                        pedAggregations,
                        terminationAggregations,
                        pcdAggregations,
                        speedAggregations,
                        yellowRedAggregations,
                        preemptAggregations,
                        priorityAggregations,
                        signalEventCountAggregations,
                        splitMonitorAggregations
                        );
                    //}
                }//);
            }
        }
        catch (System.Exception ex)
        {
            logger.LogError($"Error inserting data: {ex.Message}");
        }

        Console.WriteLine("Execution completed.");
    }

    private void GetPedAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PhasePedAggregation>> pedAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime] ,[SignalId] ,[PhaseNumber] ,[PedCycles] ,[PedDelaySum] ,[MinPedDelay] ,[MaxPedDelay] ,[ImputedPedCallsRegistered] ,[UniquePedDetections] ,[PedBeginWalkCount] ,[PedCallsRegisteredCount] ,[PedRequests] ,[ApproachId] FROM [MOE].[dbo].[PhasePedAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PhasePedAggregation> eventLogs = new List<PhasePedAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PhasePedAggregation aggregations = new PhasePedAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            PhaseNumber = (int)reader["PhaseNumber"],
                            ImputedPedCallsRegistered = (int)reader["ImputedPedCallsRegistered"],
                            PedCycles = (int)reader["PedCycles"],
                            PedDelay = (int)reader["PedDelaySum"],
                            MinPedDelay = (int)reader["MinPedDelay"],
                            MaxPedDelay = (int)reader["MaxPedDelay"],
                            UniquePedDetections = (int)reader["UniquePedDetections"],
                            PedBeginWalkCount = (int)reader["PedBeginWalkCount"],
                            PedCallsRegisteredCount = (int)reader["PedCallsRegisteredCount"],
                            PedRequests = (int)reader["PedRequests"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PhasePedAggregation> phaseCycle = new CompressedAggregations<PhasePedAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                pedAggregations.Add(phaseCycle);
            }
        }
    }

    private void GetLeftTurnAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PhaseLeftTurnGapAggregation>> leftTurnAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime] ,[SignalId] ,[PhaseNumber],[ApproachId],[GapCount1],[GapCount2],[GapCount3],[GapCount4],[GapCount5],[GapCount6],[GapCount7],[GapCount8],[GapCount9],[GapCount10],[GapCount11],[SumGapDuration1],[SumGapDuration2],[SumGapDuration3],[SumGreenTime] FROM [MOE].[dbo].[PhaseLeftTurnGapAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PhaseLeftTurnGapAggregation> eventLogs = new List<PhaseLeftTurnGapAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PhaseLeftTurnGapAggregation aggregations = new PhaseLeftTurnGapAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            PhaseNumber = (int)reader["PhaseNumber"],
                            GapCount1 = (int)reader["GapCount1"],
                            GapCount2 = (int)reader["GapCount2"],
                            GapCount3 = (int)reader["GapCount3"],
                            GapCount4 = (int)reader["GapCount4"],
                            GapCount5 = (int)reader["GapCount5"],
                            GapCount6 = (int)reader["GapCount6"],
                            GapCount7 = (int)reader["GapCount7"],
                            GapCount8 = (int)reader["GapCount8"],
                            GapCount9 = (int)reader["GapCount9"],
                            GapCount10 = (int)reader["GapCount10"],
                            GapCount11 = (int)reader["GapCount11"],
                            SumGapDuration1 = (double)reader["SumGapDuration1"],
                            SumGapDuration2 = (double)reader["SumGapDuration2"],
                            SumGapDuration3 = (double)reader["SumGapDuration3"],
                            SumGreenTime = (double)reader["SumGreenTime"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PhaseLeftTurnGapAggregation> phaseCycle = new CompressedAggregations<PhaseLeftTurnGapAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                leftTurnAggregations.Add(phaseCycle);
            }
        }
    }

    private void GetSplitFailAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<ApproachSplitFailAggregation>> splitFailAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime] ,[SignalId] ,[ApproachId] ,[PhaseNumber] ,[IsProtectedPhase] ,[SplitFailures] ,[GreenOccupancySum]  ,[RedOccupancySum] ,[GreenTimeSum] ,[RedTimeSum] ,[Cycles] FROM [MOE].[dbo].[ApproachSplitFailAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<ApproachSplitFailAggregation> eventLogs = new List<ApproachSplitFailAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        ApproachSplitFailAggregation aggregations = new ApproachSplitFailAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            PhaseNumber = (int)reader["PhaseNumber"],
                            Cycles = (int)reader["Cycles"],
                            GreenOccupancySum = (int)reader["GreenOccupancySum"],
                            RedOccupancySum = (int)reader["RedOccupancySum"],
                            GreenTimeSum = (int)reader["GreenTimeSum"],
                            RedTimeSum = (int)reader["RedTimeSum"],
                            IsProtectedPhase = (bool)reader["IsProtectedPhase"],
                            SplitFailures = (int)reader["SplitFailures"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<ApproachSplitFailAggregation> phaseCycle = new CompressedAggregations<ApproachSplitFailAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                splitFailAggregations.Add(phaseCycle);
            }
        }
    }

    private void GetPhaseCycleAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PhaseCycleAggregation>> cycleAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime],[SignalId],[ApproachId],[PhaseNumber],[RedTime],[YellowTime],[GreenTime],[TotalRedToRedCycles] ,[TotalGreenToGreenCycles]  FROM [MOE].[dbo].[PhaseCycleAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PhaseCycleAggregation> eventLogs = new List<PhaseCycleAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PhaseCycleAggregation aggregations = new PhaseCycleAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            PhaseNumber = (int)reader["PhaseNumber"],
                            RedTime = (int)reader["RedTime"],
                            YellowTime = (int)reader["YellowTime"],
                            GreenTime = (int)reader["GreenTime"],
                            TotalRedToRedCycles = (int)reader["TotalRedToRedCycles"],
                            TotalGreenToGreenCycles = (int)reader["TotalGreenToGreenCycles"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Event: {signalId}-{reader["Timestamp"]} EventCode:{reader["EventCode"]} EventParam:{reader["EventParam"]} Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PhaseCycleAggregation> phaseCycle = new CompressedAggregations<PhaseCycleAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                cycleAggregations.Add(phaseCycle);
            }
        }
    }

    private static void GetDetectorEventCountAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<DetectorEventCountAggregation>> detectorAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime] ,[SignalId] ,[ApproachId] ,[DetectorPrimaryId] ,[EventCount] FROM [MOE].[dbo].[DetectorEventCountAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<DetectorEventCountAggregation> eventLogs = new List<DetectorEventCountAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        DetectorEventCountAggregation aggregations = new DetectorEventCountAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            DetectorPrimaryId = (int)reader["DetectorPrimaryId"],
                            EventCount = (int)reader["EventCount"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<DetectorEventCountAggregation> phaseCycle = new CompressedAggregations<DetectorEventCountAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                detectorAggregations.Add(phaseCycle);
            }
        }
    }
    private static void GetPhaseTerminationAggregationAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PhaseTerminationAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime]  ,[SignalId] ,[PhaseNumber] ,[GapOuts] ,[ForceOffs] ,[MaxOuts] ,[Unknown] FROM [MOE].[dbo].[PhaseTerminationAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PhaseTerminationAggregation> eventLogs = new List<PhaseTerminationAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PhaseTerminationAggregation aggregations = new PhaseTerminationAggregation()
                        {
                            LocationIdentifier = signalId,
                            PhaseNumber = (int)reader["PhaseNumber"],
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ForceOffs = (int)reader["ForceOffs"],
                            GapOuts = (int)reader["GapOuts"],
                            MaxOuts = (int)reader["MaxOuts"],
                            Unknown = (int)reader["Unknown"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PhaseTerminationAggregation> phaseCycle = new CompressedAggregations<PhaseTerminationAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }

    private static void GetPCDAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<ApproachPcdAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime], [SignalId], [ApproachId], [PhaseNumber], [IsProtectedPhase], [ArrivalsOnGreen], [ArrivalsOnRed], [ArrivalsOnYellow], [Volume], [TotalDelay] FROM [MOE].[dbo].[ApproachPcdAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<ApproachPcdAggregation> eventLogs = new List<ApproachPcdAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        ApproachPcdAggregation aggregations = new ApproachPcdAggregation()
                        {
                            LocationIdentifier = signalId,
                            PhaseNumber = (int)reader["PhaseNumber"],
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            ArrivalsOnGreen = (int)reader["ArrivalsOnGreen"],
                            ArrivalsOnRed = (int)reader["ArrivalsOnRed"],
                            ArrivalsOnYellow = (int)reader["ArrivalsOnYellow"],
                            Volume = (int)reader["Volume"],
                            TotalDelay = (int)reader["TotalDelay"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<ApproachPcdAggregation> phaseCycle = new CompressedAggregations<ApproachPcdAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }

    private static void GetSpeedAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<ApproachSpeedAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime],[SignalId],[ApproachId],[SummedSpeed],[SpeedVolume],[Speed85th],[Speed15th] FROM [MOE].[dbo].[ApproachSpeedAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<ApproachSpeedAggregation> eventLogs = new List<ApproachSpeedAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        ApproachSpeedAggregation aggregations = new ApproachSpeedAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            Speed15th = (int)reader["Speed15th"],
                            Speed85th = (int)reader["Speed85th"],
                            SpeedVolume = (int)reader["SpeedVolume"],
                            SummedSpeed = (int)reader["SummedSpeed"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<ApproachSpeedAggregation> phaseCycle = new CompressedAggregations<ApproachSpeedAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }
    private static void GetYellowRedAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<ApproachYellowRedActivationAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime],[SignalId],[ApproachId],[PhaseNumber],[IsProtectedPhase],[SevereRedLightViolations],[TotalRedLightViolations],[YellowActivations],[ViolationTime],[Cycles] FROM [MOE].[dbo].[ApproachYellowRedActivationAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<ApproachYellowRedActivationAggregation> eventLogs = new List<ApproachYellowRedActivationAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        ApproachYellowRedActivationAggregation aggregations = new ApproachYellowRedActivationAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            ApproachId = (int)reader["ApproachId"],
                            Cycles = (int)reader["Cycles"],
                            IsProtectedPhase = (bool)reader["IsProtectedPhase"],
                            PhaseNumber = (int)reader["PhaseNumber"],
                            SevereRedLightViolations = (int)reader["SevereRedLightViolations"],
                            TotalRedLightViolations = (int)reader["TotalRedLightViolations"],
                            ViolationTime = (int)reader["ViolationTime"],
                            YellowActivations = (int)reader["YellowActivations"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<ApproachYellowRedActivationAggregation> phaseCycle = new CompressedAggregations<ApproachYellowRedActivationAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }
    private static void GetPreemptAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PreemptionAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime],[SignalId],[PreemptNumber],[PreemptRequests],[PreemptServices] FROM [MOE].[dbo].[PreemptionAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PreemptionAggregation> eventLogs = new List<PreemptionAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PreemptionAggregation aggregations = new PreemptionAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            PreemptNumber = (int)reader["PreemptNumber"],
                            PreemptRequests = (int)reader["PreemptRequests"],
                            PreemptServices = (int)reader["PreemptServices"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PreemptionAggregation> phaseCycle = new CompressedAggregations<PreemptionAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }
    private static void GetSignalEventAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<SignalEventCountAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime],[SignalId],[EventCount] FROM [MOE].[dbo].[SignalEventCountAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<SignalEventCountAggregation> eventLogs = new List<SignalEventCountAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        SignalEventCountAggregation aggregations = new SignalEventCountAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            EventCount = (int)reader["PriorityNumber"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<SignalEventCountAggregation> phaseCycle = new CompressedAggregations<SignalEventCountAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }
    private static void GetPriorityAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PriorityAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime],[SignalId],[PriorityNumber],[PriorityRequests],[PriorityServiceEarlyGreen],[PriorityServiceExtendedGreen] FROM [MOE].[dbo].[PriorityAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PriorityAggregation> eventLogs = new List<PriorityAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PriorityAggregation aggregations = new PriorityAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            PriorityNumber = (int)reader["PriorityNumber"],
                            PriorityRequests = (int)reader["PriorityRequests"],
                            PriorityServiceEarlyGreen = (int)reader["PriorityServiceEarlyGreen"],
                            PriorityServiceExtendedGreen = (int)reader["PriorityServiceExtendedGreen"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PriorityAggregation> phaseCycle = new CompressedAggregations<PriorityAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }

    private static void GetSplitMonitorAggregations(DateOnly dateToRetrieve, SqlConnection sourceConnection, List<CompressedAggregations<PhaseSplitMonitorAggregation>> terminationAggregations, string signalId)
    {
        string selectQuery = $"SELECT [BinStartTime] ,[SignalId] ,[PhaseNumber] ,[EightyFifthPercentileSplit] ,[SkippedCount] FROM [MOE].[dbo].[PhaseSplitMonitorAggregations] WHERE SignalId = '{signalId}' AND BinStartTime between '{dateToRetrieve}' AND '{dateToRetrieve.AddDays(1)}'";
        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
        {
            List<PhaseSplitMonitorAggregation> eventLogs = new List<PhaseSplitMonitorAggregation>();
            selectCommand.CommandTimeout = 120;
            Console.WriteLine($"Executing query: {selectQuery}");
            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        PhaseSplitMonitorAggregation aggregations = new PhaseSplitMonitorAggregation()
                        {
                            LocationIdentifier = signalId,
                            Start = (DateTime)reader["BinStartTime"],
                            End = ((DateTime)reader["BinStartTime"]).AddMinutes(15),
                            EightyFifthPercentileSplit = (int)reader["EightyFifthPercentileSplit"],
                            PhaseNumber = (int)reader["PhaseNumber"],
                            SkippedCount = (int)reader["SkippedCount"]
                        };

                        eventLogs.Add(aggregations);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading record: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Controller Event Logs retrieved");
            if (eventLogs.Count > 0)
            {
                CompressedAggregations<PhaseSplitMonitorAggregation> phaseCycle = new CompressedAggregations<PhaseSplitMonitorAggregation>()
                {
                    LocationIdentifier = signalId,
                    ArchiveDate = dateToRetrieve,
                    Data = eventLogs
                };
                terminationAggregations.Add(phaseCycle);
            }
        }
    }


    private void SaveAggregationsToDatabase(
        List<CompressedAggregations<PhaseCycleAggregation>> cycleAggregations,
        List<CompressedAggregations<DetectorEventCountAggregation>> detectorAggregations,
        List<CompressedAggregations<ApproachSplitFailAggregation>> splitFailAggregations,
        List<CompressedAggregations<PhaseLeftTurnGapAggregation>> leftTurnAggregations,
        List<CompressedAggregations<PhasePedAggregation>> pedAggregations,
        List<CompressedAggregations<PhaseTerminationAggregation>> terminationAggregations,
        List<CompressedAggregations<ApproachPcdAggregation>> pcdAggregations,
        List<CompressedAggregations<ApproachSpeedAggregation>> speedAggregations,
        List<CompressedAggregations<ApproachYellowRedActivationAggregation>> yellowRedAggregations,
        List<CompressedAggregations<PreemptionAggregation>> preeemptAggregations,
        List<CompressedAggregations<PriorityAggregation>> priorityAggregations,
        List<CompressedAggregations<SignalEventCountAggregation>> signalEventCountAggregations,
        List<CompressedAggregations<PhaseSplitMonitorAggregation>> splitMonitorAggregations
        )
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetService<AggregationContext>();

            if (context != null)
            {
                foreach (var aggregation in cycleAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in detectorAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in splitFailAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in leftTurnAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in pedAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in terminationAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in pcdAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in speedAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in yellowRedAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in preeemptAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in priorityAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in signalEventCountAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                foreach (var aggregation in splitMonitorAggregations)
                {
                    context.CompressedAggregations.Add(aggregation);
                }
                context.SaveChanges();
            }
        }
        cycleAggregations.Clear();
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

    public static byte[] SerializeAndCompress<T>(T data)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Arrays,
            // Add Converters with StringEnumConverter to handle enum serialization as integers
            Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter { AllowIntegerValues = true } }
        };
        string json = JsonConvert.SerializeObject(data, settings);
        return GZipCompressToByte(json);
    }

}
