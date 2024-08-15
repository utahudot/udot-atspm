using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementAggregation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpeedManagementImporter.Business.Pems;
using System.Collections.Concurrent;

namespace SpeedManagementImporter.Services.Pems
{
    public class PemsDownloaderService : IDataDownloader
    {
        private ISegmentEntityRepository segmentEntityRepository;
        private IHourlySpeedRepository hourlySpeedRepository;

        static readonly int confidenceId = 4;
        static readonly int sourceId = 2;

        public PemsDownloaderService(ISegmentEntityRepository segmentEntityRepository, IHourlySpeedRepository hourlySpeedRepository)
        {
            this.segmentEntityRepository = segmentEntityRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string apiKey = config.GetSection("AppSettings:ApiKey").Value;


            List<SegmentEntityWithSpeedAndAlternateIdentifier> routeEntities = await segmentEntityRepository.GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(sourceId);

            var alternateIdentifiers = routeEntities.Where(r => r.AlternateIdentifier != null).Select(r => r.AlternateIdentifier).Distinct().ToList();

            var speeds = new ConcurrentBag<HourlySpeed>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 1 // Set the desired parallelism level here
            };

            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                Parallel.ForEach(alternateIdentifiers, parallelOptions, async pemsRoute =>
                {
                    var split = pemsRoute.Split('-');
                    if (split.Length > 2)
                    {
                        split = split.Skip(1).ToArray();
                    }
                    var fwy = new string(split[0].Where(char.IsAsciiDigit).ToArray());
                    var flowUrl = $"https://udot.iteris-pems.com/?srq=rest&api_key={apiKey}&service=bts.freeway.getSpatialHourlySummary&from={date:yyyyMMdd}&to={date.AddDays(1):yyyyMMdd}&dow_0=on&dow_1=on&dow_2=on&dow_3=on&dow_4=on&dow_5=on&dow_6=on&fwy={fwy}&dir={split[1]}&lanes=1&start_pm=0&end_pm=349.440&q=flow&returnformat=json";
                    var pemsFlows = await GetPemsFlows(flowUrl);
                    for (int i = 0; i <= 22; i++)
                    {
                        var tempTime = new TimeOnly(i, 0);
                        var statisticsUrl = $"https://udot.iteris-pems.com/?srq=rest&api_key={apiKey}&service=bts.station.getStatistics&from_date={date:yyyyMMdd}&to_date={date.AddDays(1):yyyyMMdd}&from_tod={i}&to_tod={i + 1}&dow_0=on&dow_1=on&dow_2=on&dow_3=on&dow_4=on&dow_5=on&fwy={fwy}&dir={split[1]}&lanes=agg&start_pm=0&end_pm=349.440&q=speed&granularity=hour&returnformat=json";
                        var pemsStatistics = await GetPemsStatisticsAsync(statisticsUrl);
                        if (pemsStatistics == null || pemsStatistics.Count == 0)
                            continue;

                        Dictionary<Guid, List<SegmentEntityWithSpeedAndAlternateIdentifier>> routesAlternateIdentifier = routeEntities.Where(r => r.AlternateIdentifier == pemsRoute).GroupBy(re => re.SegmentId).ToDictionary(group => group.Key, group => group.ToList());

                        foreach (var route in routesAlternateIdentifier)
                        {
                            try
                            {
                                var routeId = route.Key;
                                List<SegmentEntityWithSpeedAndAlternateIdentifier> routeEntityList = route.Value;

                                var speedLimit = routeEntityList.Select(r => r.SpeedLimit).Distinct().First();

                                List<long> stationIds = routeEntityList.Select(routeEntity => routeEntity.EntityId).ToList();
                                if (stationIds == null || stationIds.Count <= 0)
                                    continue;

                                List<StationMeasurement> statisticsByRoute = pemsStatistics.Where(p => p != null && stationIds.Contains(Convert.ToInt32(p.station_id))).ToList();

                                if (statisticsByRoute == null || statisticsByRoute.Count == 0)
                                    continue;
                                int? averageFlowForHour = null;
                                if (pemsFlows != null)
                                {
                                    var flowByRoute = pemsFlows.Where(p => stationIds.Contains(Convert.ToInt32(p.station))).ToList();
                                    averageFlowForHour = GetFlowValuesForHour(flowByRoute, i);
                                }
                                List<Quantity> quantities = statisticsByRoute
                                    .SelectMany(s => s.quantities.quantity).ToList();
                                if (!quantities.Any()) continue;

                                List<double> percentile15List = quantities
                                    .Where(s => s.label == "15th" && s.value != null)
                                    .Select(s => s.value.Value)
                                    .ToList();

                                List<double> percentile85List = quantities
                                    .Where(s => s.label == "85th" && s.value != null)
                                    .Select(s => s.value.Value)
                                    .ToList();
                                var percentile95List = quantities
                                    .Where(s => s.label == "95th" && s.value != null)
                                    .Select(s => s.value.Value)
                                    .ToList();
                                var means = quantities
                                    .Where(s => s.label == "Mean" && s.value != null)
                                    .Select(s => s.value.Value)
                                    .ToList();
                                int? fifteenthSpeed = GetNullSafeAverage(percentile15List);

                                int? eightyFifthSpeed = GetNullSafeAverage(percentile85List);

                                int? ninetyFifthSpeed = GetNullSafeAverage(percentile95List);

                                int? mean = GetNullSafeAverage(means);
                                if (mean == null) continue;

                                long? violation = null;
                                if (mean != null)
                                    violation = mean > speedLimit && speedLimit > 0 ? mean - speedLimit : 0;

                                var speed = new HourlySpeed
                                {
                                    Date = date,
                                    BinStartTime = date,
                                    SegmentId = routeId,
                                    SourceId = sourceId,
                                    ConfidenceId = confidenceId,
                                    Average = mean.Value,
                                    FifteenthSpeed = fifteenthSpeed,
                                    EightyFifthSpeed = eightyFifthSpeed,
                                    NinetyFifthSpeed = ninetyFifthSpeed,
                                    Violation = violation,
                                    Flow = averageFlowForHour
                                };
                                speeds.Add(speed);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error adding speed record for route {route.Key} {sourceId} {route.Value.First().AlternateIdentifier} {ex.Message}");
                            }
                        }
                    }
                    if (speeds.Count > 1000)
                    {
                        await hourlySpeedRepository.AddHourlySpeedsAsync(speeds.ToList());
                        speeds.Clear();
                    }
                });
            }
            await hourlySpeedRepository.AddHourlySpeedsAsync(speeds.ToList());
        }

        private static int? GetNullSafeAverage(List<double> statisticsByRoute)
        {
            if (statisticsByRoute == null || !statisticsByRoute.Any())
                return null;
            double? eightyFifthSpeed = statisticsByRoute.Average();
            return eightyFifthSpeed != null ? (int)Math.Round(eightyFifthSpeed.Value) : null;
        }

        private static int? GetFlowValuesForHour(List<Station> flowByRoute, int i)
        {
            if (flowByRoute == null || flowByRoute.Count == 0)
                return null;
            switch (i)
            {
                case 0:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_00));
                case 1:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_01));
                case 2:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_02));
                case 3:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_03));
                case 4:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_04));
                case 5:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_05));
                case 6:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_06));
                case 7:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_07));
                case 8:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_08));
                case 9:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_09));
                case 10:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_10));
                case 11:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_11));
                case 12:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_12));
                case 13:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_13));
                case 14:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_14));
                case 15:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_15));
                case 16:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_16));
                case 17:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_17));
                case 18:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_18));
                case 19:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_19));
                case 20:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_20));
                case 21:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_21));
                case 22:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_22));
                case 23:
                    return (int)Math.Round(flowByRoute.Average(p => p.hr_23));
                default: return null;
            }
        }
        private static async Task<List<Station>> GetPemsFlows(string flowUrl, int retry = 0)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(flowUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(jsonResponse))
                            return null;

                        // Deserialize JSON using Newtonsoft.Json
                        var rootObject = JsonConvert.DeserializeObject<RootObjectFlow>(jsonResponse);
                        return rootObject.stationsHourlySummary.stations;
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                        if (retry < 4)
                        {
                            return await GetPemsFlows(flowUrl, retry++);
                        }
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }

        private static async Task<List<StationMeasurement>> GetPemsStatisticsAsync(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(jsonResponse))
                            return null;

                        // Deserialize JSON using Newtonsoft.Json
                        var rootObject = JsonConvert.DeserializeObject<RootObjectSpeed>(jsonResponse);
                        return rootObject.measurements.station;
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}
