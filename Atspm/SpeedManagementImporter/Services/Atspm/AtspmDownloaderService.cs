using Microsoft.Extensions.Configuration;
using SpeedManagementImporter.Business.Atspm;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter.Services.Atspm
{
    public class AtspmDownloaderService : IDataDownloader
    {
        private readonly int sourceId = 1;
        private readonly string sourceConnectionString;
        private ISegmentEntityRepository segmentEntityRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private IConfiguration configuration;

        public AtspmDownloaderService(ISegmentEntityRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository, IConfiguration configuration)
        {
            this.segmentEntityRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.configuration = configuration;
            sourceConnectionString = this.configuration["Atspm:ConnectionString"];
        }

        public async Task Download(DateTime startDate, DateTime endDate, List<string>? providedSegments)
        {
            var segmentEntities = await segmentEntityRepository.GetEntitiesWithSpeedForSourceId(sourceId);
            var segments = segmentEntities.GroupBy(r => r.SegmentId).ToList();

            var speeds = new ConcurrentBag<HourlySpeed>();

            if (sourceConnectionString == null)
            {
                return;
            }

            for (DateTime date = startDate; date < endDate; date = date.AddHours(1))
            {
                foreach (var segment in segments)
                {
                    List<ApproachSpeed> approachSpeeds = new List<ApproachSpeed>();
                    //EntityId needs to be of type long
                    var entityIdsForSegment = segment.Select(s => Int64.Parse(s.EntityId)).ToList();
                    string selectQuery = $@"SELECT [BinStartTime]
                                          ,[SignalId]
                                          ,[ApproachId]
                                          ,[SummedSpeed]
                                          ,[SpeedVolume]
                                          ,[Speed85th]
                                          ,[Speed15th]
                                      FROM [MOE].[dbo].[ApproachSpeedAggregations]
                                      WHERE BinStartTime BETWEEN '{date}' AND '{date.AddHours(1)}'
                                      AND ApproachId IN ({string.Join(",", entityIdsForSegment)})";

                    bool success = false;
                    while (!success)
                    {
                        try
                        {
                            using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
                            {
                                await sourceConnection.OpenAsync();

                                using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
                                {
                                    using (SqlDataReader reader = await selectCommand.ExecuteReaderAsync())
                                    {
                                        while (await reader.ReadAsync())
                                        {
                                            approachSpeeds.Add(new ApproachSpeed
                                            {
                                                ApproachId = (int)reader["ApproachId"],
                                                SignalId = (string)reader["SignalId"],
                                                Speed15th = (int)reader["Speed15th"],
                                                Speed85th = (int)reader["Speed85th"],
                                                SpeedVolume = (int)reader["SpeedVolume"],
                                                SummedSpeed = (int)reader["SummedSpeed"]
                                            });
                                        }
                                    }
                                }
                            }
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading from database: {ex.Message}. Retrying in 5 minutes...");
                            approachSpeeds.Clear(); // Clear the list to remove partially read data
                            await Task.Delay(TimeSpan.FromMinutes(5));
                        }
                    }

                    approachSpeeds = approachSpeeds.Where(a => a.SpeedVolume > 0 && a.SummedSpeed > 0).ToList();
                    if (approachSpeeds.Count == 0) { continue; }

                    var summedSpeed = approachSpeeds.Sum(a => a.SummedSpeed);
                    var summedVolume = approachSpeeds.Sum(a => a.SpeedVolume);
                    double averageSpeed = summedSpeed / summedVolume;
                    double eightyFifthSpeed = approachSpeeds.Select(a => a.SpeedVolume * a.Speed85th).Sum() / summedVolume;
                    double fifteenthSpeed = approachSpeeds.Select(a => a.SpeedVolume * a.Speed15th).Sum() / summedVolume;
                    var speedLimit = segment.Select(route => route.SpeedLimit).FirstOrDefault();
                    speeds.Add(new HourlySpeed
                    {
                        Date = date,
                        BinStartTime = date,
                        Average = approachSpeeds.Sum(a => a.SummedSpeed) / approachSpeeds.Sum(a => a.SpeedVolume),
                        EightyFifthSpeed = Convert.ToInt32(Math.Round(eightyFifthSpeed)),
                        FifteenthSpeed = Convert.ToInt32(Math.Round(fifteenthSpeed)),
                        SegmentId = segment.Key,
                        Violation = Convert.ToInt64(speedLimit < eightyFifthSpeed && speedLimit != 0 ? (long)eightyFifthSpeed - speedLimit : 0),
                        Flow = summedVolume,
                        SourceId = sourceId,
                        PercentObserved = 4
                    });

                }

                Console.WriteLine($"{date} for source ATSPM added to list");

                if (speeds.Count > 10000)
                {
                    await TryWriteToDatabaseAsync(speeds);
                }
            }
            await TryWriteToDatabaseAsync(speeds);
        }

        private async Task TryWriteToDatabaseAsync(ConcurrentBag<HourlySpeed> speeds)
        {
            bool success = false;
            while (!success)
            {
                try
                {
                    await hourlySpeedRepository.AddHourlySpeedsAsync(speeds.ToList());
                    speeds.Clear();
                    Console.WriteLine("List written to database");
                    success = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to database: {ex.Message}. Retrying in 5 minutes...");
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }
    }
}
