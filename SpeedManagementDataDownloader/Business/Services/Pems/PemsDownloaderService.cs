using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpeedManagementDataDownloader.Business.Common.DataDownloader;
using SpeedManagementDataDownloader.Common.Dtos;
using SpeedManagementDataDownloader.Common.EntityTable;
using SpeedManagementDataDownloader.Common.HourlySpeeds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Business.Services.Pems
{
    public class PemsDownloaderService : IDataDownloader
    {
        private IRouteEntityTableRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;

        static readonly int confidenceId = 4;
        static readonly int sourceId = 2;

        public PemsDownloaderService(IRouteEntityTableRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository)
        {
            // Initialize the DbContext and repository
            //var dbContext = new SpeedContext();
            this.routeEntityTableRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {
            IConfiguration config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Use AppContext.BaseDirectory
                .AddJsonFile("appsettings.json")
                .Build();

            string apiKey = config.GetSection("AppSettings:ApiKey").Value;

            List<RouteEntityWithSpeedAndAlternateIdentifier> routeEntities = await routeEntityTableRepository.GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(sourceId);

            var alternateIdentifiers = routeEntities.Where(r => r.AlternateIdentifier != null).Select(r => r.AlternateIdentifier).Distinct().ToList();

            var speeds = new ConcurrentBag<HourlySpeed>();

            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                foreach (var pemsRoute in alternateIdentifiers)
                {
                    var split = pemsRoute.Split('-');
                    var flowUrl = $"https://udot.iteris-pems.com/?srq=rest&api_key={apiKey}&service=bts.freeway.getSpatialHourlySummary&from={date:yyyyMMdd}&to={date.AddDays(1):yyyyMMdd}&dow_0=on&dow_1=on&dow_2=on&dow_3=on&dow_4=on&dow_5=on&dow_6=on&fwy={split[0]}&dir={split[1]}&lanes=1&start_pm=0&end_pm=349.440&q=flow&returnformat=json";
                    var pemsFlows = await GetPemsFlows(flowUrl);
                    for (int i = 0; i <= 22; i++)
                    {
                        var tempTime = new TimeOnly(i, 0);
                        var statisticsUrl = $"https://udot.iteris-pems.com/?srq=rest&api_key={apiKey}&service=bts.station.getStatistics&from_date={date:yyyyMMdd}&to_date={date.AddDays(1):yyyyMMdd}&from_tod={i}&to_tod={i + 1}&dow_0=on&dow_1=on&dow_2=on&dow_3=on&dow_4=on&dow_5=on&fwy={split[0]}&dir={split[1]}&lanes=1&start_pm=0&end_pm=349.440&q=speed&granularity=hour&returnformat=json";
                        var pemsStatistics = await GetPemsStatisticsAsync(statisticsUrl);
                        if (pemsStatistics == null || pemsStatistics.Count == 0)
                            continue;

                        Dictionary<int, List<RouteEntityWithSpeedAndAlternateIdentifier>> routesAlternateIdentifier = routeEntities.Where(r => r.AlternateIdentifier == pemsRoute).GroupBy(re => re.RouteId).ToDictionary(group => group.Key, group => group.ToList());

                        foreach (var route in routesAlternateIdentifier)
                        {
                            try
                            {
                                var routeId = route.Key;
                                List<RouteEntityWithSpeedAndAlternateIdentifier> routeEntityList = route.Value;

                                //The SpeedLimit should be the same the entire route.
                                var speedLimit = routeEntityList.Select(r => r.SpeedLimit).Distinct().First();

                                //Stations are the grouping of the entityIds
                                List<int> stationIds = routeEntityList.Select(routeEntity => routeEntity.EntityId).ToList();
                                if (stationIds == null || stationIds.Count <= 0)
                                    continue;

                                List<StationMeasurement> statisticsByRoute = pemsStatistics.Where(p => p != null && stationIds.Contains(Convert.ToInt32(p.station_id))).ToList();

                                if (statisticsByRoute == null
                                    || statisticsByRoute.Count == 0)
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

                                int? violation = null;
                                if (mean != null)
                                    violation = mean > speedLimit && speedLimit > 0 ? mean - speedLimit : 0;

                                var speed = new HourlySpeed
                                {
                                    Date = date,
                                    BinStartTime = date,
                                    RouteId = routeId,
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
                                //I can just take the first AlternateIdentifier because there should only be one AlternateIdentifier per route.
                                Console.WriteLine($"Error adding speed record for route {route.Key} {sourceId} {route.Value.First().AlternateIdentifier} {ex.Message}");
                            }
                        }
                    }
                    //Console.WriteLine("Speed records added successfully!");
                    if (speeds.Count > 10000)
                    {
                        await hourlySpeedRepository.AddHourlySpeedsAsync(speeds.ToList());
                        speeds.Clear();
                    }
                }
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
        private static async Task<List<Station>> GetPemsFlows(string flowUrl)
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
