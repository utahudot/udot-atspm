using SpeedManagementDataDownloader.Business.Common.DataDownloader;
using SpeedManagementDataDownloader.Common.EntityTable;
using SpeedManagementDataDownloader.Common.HourlySpeeds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Business.Services.Atspm
{
    public class AtspmDownloaderService : IDataDownloader
    {
        private readonly int sourceId = 1;
        private readonly string sourceConnectionString = "Data Source=srwtcmoedb.utah.utad.state.ut.us;Initial Catalog=MOE;User ID=spm;Password=UdotNewMoeDb!9;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private IRouteEntityTableRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;

        public AtspmDownloaderService(IRouteEntityTableRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository)
        {
            this.routeEntityTableRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {
            var routeEntities = await routeEntityTableRepository.GetEntitiesWithSpeedForSourceId(sourceId);
            var routes = routeEntities.GroupBy(r => r.RouteId).ToList();

            var speeds = new ConcurrentBag<HourlySpeed>();
            for (DateTime date = startDate; date < endDate; date = date.AddHours(1))
            {
                foreach (var route in routes)
                {

                    List<ApproachSpeed> approachSpeeds = new List<ApproachSpeed>();
                    string selectQuery = $@"SELECT [BinStartTime]
                                          ,[SignalId]
                                          ,[ApproachId]
                                          ,[SummedSpeed]
                                          ,[SpeedVolume]
                                          ,[Speed85th]
                                          ,[Speed15th]
                                      FROM [MOE].[dbo].[ApproachSpeedAggregations]
                                      WHERE BinStartTime BETWEEN '{date}' AND '{date.AddHours(1)}'
                                      AND ApproachId IN ({string.Join(",", route.Select(r => r.EntityId))})";



                    using (SqlConnection sourceConnection = new SqlConnection(sourceConnectionString))
                    {
                        sourceConnection.Open();

                        using (SqlCommand selectCommand = new SqlCommand(selectQuery, sourceConnection))
                        {

                            using (SqlDataReader reader = selectCommand.ExecuteReader())
                            {
                                using (SqlConnection targetConnection = new SqlConnection(sourceConnectionString))
                                {
                                    targetConnection.Open();
                                    while (reader.Read())
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
                    }
                    approachSpeeds = approachSpeeds.Where(a => a.SpeedVolume > 0 && a.SummedSpeed > 0).ToList();
                    if (approachSpeeds.Count == 0) { continue; }
                    var summedSpeed = approachSpeeds.Sum(a => a.SummedSpeed);
                    var summedVolume = approachSpeeds.Sum(a => a.SpeedVolume);
                    double averageSpeed = summedSpeed / summedVolume;
                    double eightyFifthSpeed = approachSpeeds.Select(a => a.SpeedVolume * a.Speed85th).Sum() / summedVolume;
                    double fifteenthSpeed = approachSpeeds.Select(a => a.SpeedVolume * a.Speed15th).Sum() / summedVolume;
                    var speedLimit = route.Select(route => route.SpeedLimit).FirstOrDefault();
                    speeds.Add(new HourlySpeed
                    {
                        Date = date,
                        BinStartTime = date,
                        Average = approachSpeeds.Sum(a => a.SummedSpeed) / approachSpeeds.Sum(a => a.SpeedVolume),
                        EightyFifthSpeed = Convert.ToInt32(Math.Round(eightyFifthSpeed)),
                        FifteenthSpeed = Convert.ToInt32(Math.Round(fifteenthSpeed)),
                        RouteId = route.Key,
                        Violation = Convert.ToInt64(speedLimit < eightyFifthSpeed && speedLimit != 0 ? (long)eightyFifthSpeed - speedLimit : 0),
                        Flow = summedVolume,
                        SourceId = 3,
                        ConfidenceId = 4
                    });

                }
                Console.WriteLine($"{date} for source ATSPM added to list");
                if (speeds.Count > 10000)
                {
                    await hourlySpeedRepository.AddHourlySpeedsAsync(speeds.ToList());
                    speeds.Clear();
                }
            }
            await hourlySpeedRepository.AddHourlySpeedsAsync(speeds.ToList());
            //await InsertSpeedRecords(speedRepository, speeds);
        }
    }
}
