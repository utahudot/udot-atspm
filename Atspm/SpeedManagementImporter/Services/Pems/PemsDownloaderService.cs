using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeedManagementImporter.Business.Pems;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Importer;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter.Services.Pems
{
    public class PemsDownloaderService : IDataDownloader
    {
        private ISegmentEntityRepository segmentEntityRepository;
        private IHourlySpeedRepository hourlySpeedRepository;
        private IConfigurationRoot configuration;

        static readonly int sourceId = 2;
        private string sessionId;

        public PemsDownloaderService(ISegmentEntityRepository segmentEntityRepository, IHourlySpeedRepository hourlySpeedRepository, IConfigurationRoot configuration)
        {
            this.segmentEntityRepository = segmentEntityRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.configuration = configuration;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {
            string apiKey = configuration["Pems:ApiKey"];
            var nullPems = 0;

            if (apiKey == null)
            {
                return;
            }


            List<SegmentEntityWithSpeedAndAlternateIdentifier> routeEntities = await segmentEntityRepository.GetEntitiesWithSpeedAndAlternateIdentifierForSourceId(sourceId);

            var alternateIdentifiers = routeEntities.Where(r => r.AlternateIdentifier != null).ToList().Select(r => r.AlternateIdentifier).Distinct().ToList();

            var tasks = new List<Task>();

            //foreach (var pemsRoute in alternateIdentifiers)
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var localSpeeds = new List<HourlySpeed>();

                    //for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                    foreach (var pemsRoute in alternateIdentifiers)
                    {
                        var split = pemsRoute.Split('-');
                        if (split.Length > 2)
                        {
                            split = split.Skip(1).ToArray();
                        }
                        var fwy = new string(split[0].Where(char.IsAsciiDigit).ToArray());
                        var flowUrl = $"https://udot.iteris-pems.com/?srq=rest&api_key={apiKey}&service=bts.freeway.getSpatialHourlySummary&from={date:yyyyMMdd}&to={date.AddDays(1):yyyyMMdd}&dow_0=on&dow_1=on&dow_2=on&dow_3=on&dow_4=on&dow_5=on&dow_6=on&fwy={fwy}&dir={split[1]}&lanes=1&start_pm=0&end_pm=349.440&q=flow&returnformat=json";
                        var pemsFlows = await GetPemsFlows(flowUrl);

                        if (pemsFlows != null)
                        {
                            Dictionary<Guid, List<SegmentEntityWithSpeedAndAlternateIdentifier>> routesAlternateIdentifier = routeEntities.Where(r => r.AlternateIdentifier == pemsRoute).ToList().GroupBy(re => re.SegmentId).ToList().ToDictionary(group => group.Key, group => group.ToList());

                            Dictionary<String, ViolationsForEachHour> violationsForToday = new Dictionary<String, ViolationsForEachHour>();

                            foreach (var pemsFlow in pemsFlows)
                            {
                                //speedlimit
                                //All I need to do Is pull out the speed limit
                                var stationId = Convert.ToInt64(pemsFlow.station);
                                var potentialSpeedlimits = routeEntities.Where(route => route.EntityId == stationId).Distinct().ToList();
                                if (potentialSpeedlimits.Count > 1)
                                {
                                    Console.WriteLine($"We got multiple speedlimits for this guy - {stationId}");
                                }
                                var speedLimit = (int)potentialSpeedlimits.First().SpeedLimit;

                                var hourlyViolations = await GetViolationsForEachHourInTheDay(date, pemsFlow.station, speedLimit, true);
                                violationsForToday.Add(pemsFlow.station, hourlyViolations);
                            }

                            for (int i = 0; i <= 22; i++)
                            {
                                var toTime = i + 1;
                                var toDate = date.AddDays(1);
                                DateTime binStartTime = DateOnly.FromDateTime(DateTime.Today).ToDateTime(new TimeOnly(i, 0));
                                var statisticsUrl = $"https://udot.iteris-pems.com/?srq=rest&api_key={apiKey}&service=bts.station.getStatistics&from_date={date:yyyyMMdd}&to_date={toDate:yyyyMMdd}&from_tod={i}&to_tod={toTime}&dow_0=on&dow_1=on&dow_2=on&dow_3=on&dow_4=on&dow_5=on&fwy={fwy}&dir={split[1]}&lanes=agg&start_pm=0&end_pm=349.440&q=speed&granularity=hour&returnformat=json";
                                var pemsStatistics = await GetPemsStatisticsAsync(statisticsUrl, 0);
                                if (pemsStatistics == null || pemsStatistics.Count() == 0)
                                    continue;

                                //Use the percent observed... from the pemsStats
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
                                        int extremeViolation = 0;
                                        int violation = 0;
                                        int dataQuality = 0;
                                        //TODO check if pemsFlow or PemsStat should be used.
                                        if (pemsFlows != null)
                                        {
                                            var flowByRoute = pemsFlows.Where(p => stationIds.Contains(Convert.ToInt32(p.station))).ToList();
                                            averageFlowForHour = GetFlowValuesForHour(flowByRoute, i);
                                            foreach (var currentFlowRoute in flowByRoute)
                                            {
                                                //See how often we hit this and if it is worth it to just have it in here...
                                                //var hourlyViolations = await GetViolationsForEachHourInTheDay(date, currentFlowRoute.station, speedLimit, true);
                                                extremeViolation = extremeViolation + violationsForToday[currentFlowRoute.station].GetExtremeViolation(i);
                                                violation = violation + violationsForToday[currentFlowRoute.station].GetViolation(i);
                                                dataQuality = dataQuality + violationsForToday[currentFlowRoute.station].GetDataQuality(i);
                                            }
                                            if (dataQuality > 0 && flowByRoute.Count > 0)
                                            {
                                                dataQuality = dataQuality / flowByRoute.Count;
                                            }
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

                                        var speed = new HourlySpeed
                                        {
                                            Date = date,
                                            BinStartTime = binStartTime,
                                            SegmentId = routeId,
                                            SourceId = sourceId,
                                            ConfidenceId = dataQuality,
                                            Average = mean.Value,
                                            FifteenthSpeed = fifteenthSpeed,
                                            EightyFifthSpeed = eightyFifthSpeed,
                                            NinetyFifthSpeed = ninetyFifthSpeed,
                                            Violation = violation,
                                            ExtremeViolation = extremeViolation,
                                            Flow = averageFlowForHour
                                        };
                                        localSpeeds.Add(speed);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error adding speed record for route {route.Key} {sourceId} {route.Value.First().AlternateIdentifier} {ex.Message}");
                                    }
                                }
                            }
                            // Check if the batch size is reached
                            if (localSpeeds.Count >= 1000)
                            {
                                await hourlySpeedRepository.AddHourlySpeedsAsync(localSpeeds);
                                Console.WriteLine("\n Writing 1000 lines with more to go.\n");
                                localSpeeds.Clear(); // Clear the batch after upload
                            }
                        }
                        else
                        {
                            nullPems++;
                        }
                    }

                    //Now we append that list
                    // Upload any remaining speeds in the local batch
                    if (localSpeeds.Count > 0)
                    {
                        await hourlySpeedRepository.AddHourlySpeedsAsync(localSpeeds);
                        Console.WriteLine($"\n\n Writing remaining {localSpeeds.Count} speeds.\n\n");
                    }
                }));
                if (tasks.Count >= 2) // Limit parallelism to 2
                {
                    await Task.WhenAny(tasks); // Wait for one task to complete before starting another
                    tasks.RemoveAll(t => t.IsCompleted);
                }
            }

            await Task.WhenAll(tasks); // Wait for all remaining tasks to complete
            Console.WriteLine($"Pems was null {nullPems} times.");
            Console.WriteLine("Complete");
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
                string debugString = "";

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(flowUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(jsonResponse))
                            return null;

                        debugString = jsonResponse;
                        var json = JObject.Parse(jsonResponse);
                        // Deserialize JSON using Newtonsoft.Json
                        if (json["stationsHourlySummary"].Count() == 0)
                        {
                            return null;
                        }
                        var rootObject = JsonConvert.DeserializeObject<RootObjectFlow>(jsonResponse);
                        return rootObject.stationsHourlySummary.stations;
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error(Flow): {response.StatusCode} - {response.ReasonPhrase} NUMBER RETRY - {retry}");
                        if (retry < 10)
                        {

                            return await GetPemsFlows(flowUrl, retry++);
                        }
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message} WITH JSON STRING - {debugString}\n\n");
                    return null;
                }
            }
        }

        private static async Task<List<StationMeasurement>> GetPemsStatisticsAsync(string url, int retry)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.Timeout = TimeSpan.FromMinutes(2);

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(jsonResponse))
                            return null;

                        var json = JObject.Parse(jsonResponse);
                        // Deserialize JSON using Newtonsoft.Json
                        if (json["measurements"].Count() == 0)
                        {
                            return null;
                        }
                        // Deserialize JSON using Newtonsoft.Json
                        var rootObject = JsonConvert.DeserializeObject<RootObjectSpeed>(jsonResponse);
                        return rootObject.measurements.station;
                    }
                    else
                    {
                        Console.WriteLine($"HTTP Error (Stats): {response.StatusCode} - {response.ReasonPhrase}NUMBER RETRY - {retry}");
                        if (retry < 10)
                        {

                            return await GetPemsStatisticsAsync(url, retry++);
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

        private async Task<ViolationsForEachHour> GetViolationsForEachHourInTheDay(DateTime originalDateTime, string stationId, long speedLimit, Boolean freeway, int retry = 0)
        {
            if (this.sessionId.IsNullOrEmpty())
            {
                this.sessionId = await LoginToPems();
            }
            try
            {
                var _cookieContainer = new CookieContainer();
                var handler = new HttpClientHandler
                {
                    CookieContainer = _cookieContainer,
                    AllowAutoRedirect = true, // Follow redirects if necessary
                    UseCookies = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                using (var httpClient = new HttpClient(handler))
                {

                    int desiredHour = 00;
                    int desiredMinute = 00;
                    DateTime date = new DateTime(
                        originalDateTime.Year,
                        originalDateTime.Month,
                        originalDateTime.Day,
                        desiredHour,
                        desiredMinute,
                        0 // seconds
                    );

                    string formattedDate = date.ToString("MM/dd/yyyy HH:mm");
                    string urlEncodedDate = HttpUtility.UrlEncode(formattedDate);
                    long unixTime = ((DateTimeOffset)date).ToUnixTimeSeconds();

                    DateTime tilDate = date.AddDays(1);
                    string formattedTilDate = tilDate.ToString("MM/dd/yyyy HH:mm");
                    string urlEncodedTilDate = HttpUtility.UrlEncode(formattedTilDate);
                    long unixTimeTil = ((DateTimeOffset)date.AddDays(1)).ToUnixTimeSeconds();

                    var downloadDataUrl = $"https://udot.iteris-pems.com/?report_form=1&dnode=VDS&content=detector_health&tab=dh_raw&export=text&station_id={stationId}&s_time_id={unixTime}&s_time_id_f={urlEncodedDate}&e_time_id={unixTimeTil}&e_time_id_f={urlEncodedTilDate}&lanes={stationId}-0&q=speed_used&q2=flow&gn=sec";

                    //Include the PHPSESSID in the Cookie header for the next request
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/signed-exchange", 0.7));

                    httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                    httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                    httpClient.DefaultRequestHeaders.Add("Priority", "u=0, i");

                    httpClient.DefaultRequestHeaders.Add("Cookie", sessionId);

                    //Call the endpoint
                    var TwentySecondResults = await httpClient.GetAsync(downloadDataUrl);
                    var TwentySecondSpeedString = await TwentySecondResults.Content.ReadAsStringAsync();
                    List<List<String>> rowsOfColumns = SplitToTimeGrid(TwentySecondSpeedString);
                    ViolationsForEachHour violationsForEachHour = GridToObject(speedLimit, freeway, rowsOfColumns, originalDateTime);

                    return violationsForEachHour;
                }
            }
            catch (Exception ex)
            {
                if (retry > 2)
                {
                    Console.WriteLine($"Violations - We had an issue pulling the 20 second data from pems for this station - {stationId}, therefore we failed out and returned 0's.");
                    return new ViolationsForEachHour();
                }
                Console.WriteLine("threw exception" + ex.Message);
                this.sessionId = await LoginToPems();
                return await GetViolationsForEachHourInTheDay(originalDateTime, stationId, speedLimit, freeway, retry++);
            }
        }

        private static ViolationsForEachHour GridToObject(long speedLimit, bool freeway, List<List<string>> rowsOfColumns, DateTime date)
        {
            ViolationsForEachHour violationsForEachHour = new ViolationsForEachHour();
            violationsForEachHour.SpeedLimit = speedLimit;
            violationsForEachHour.Date = date;
            if (rowsOfColumns.Count == 0)
            {
                return violationsForEachHour;
            }

            //These are important because this is how the grid is populated
            //Always skip the first location because that is the time
            //Speed comes first
            var startOfSpeedLocation = 1;
            //This is important for looping and getting each lanes speed stuff
            var lanesCount = (rowsOfColumns[0].Count - 1) / 2;
            //Then comes flow
            var startOfFlowLocation = ((rowsOfColumns[0].Count - 1) / 2) + 1;

            for (var hour = 0; hour <= 23; hour++)
            {
                //Get out all the rows where the time hour is equal to the hour I am looking for.
                var dataForTheHour = rowsOfColumns
                    .Where(row => DateTime.TryParse(row[0], out var dateTime) && dateTime.Hour == hour)
                    .ToList();
                //These will be the combine violations that will be added to
                var combinedSpeedViolations = 0;
                var combinedExtremeSpeedViolations = 0;
                var dataInFlowGreaterThan0 = 0;
                //Cycle through each lane to get the violations
                for (var lanes = 0; lanes < lanesCount; lanes++)
                {
                    int speedLane = startOfSpeedLocation + lanes;
                    int flowLane = startOfFlowLocation + lanes;

                    var speedViolations = dataForTheHour?
                        .Where(i => double.TryParse(i[speedLane], out double speed) && speed >= (speedLimit + 2))
                        .ToList();
                    int speedViolationsFlow = speedViolations?
                        .Sum(i => int.TryParse(i[flowLane], out int flow) ? flow : 0) ?? 0;

                    var extremeSpeedViolations = dataForTheHour?
                        .Where(i => double.TryParse(i[speedLane], out double speed) && speed >= (speedLimit + 7))
                        .ToList();
                    int extremeSpeedViolationsFlow = extremeSpeedViolations?
                        .Sum(i => int.TryParse(i[flowLane], out int flow) ? flow : 0) ?? 0;

                    //In the case of it being a freeway we need to treat it differently
                    if (freeway)
                    {
                        extremeSpeedViolations = dataForTheHour?
                        .Where(i => double.TryParse(i[speedLane], out double speed) && speed >= (speedLimit + 10))
                        .ToList();
                        extremeSpeedViolationsFlow = extremeSpeedViolations?
                            .Sum(i => int.TryParse(i[flowLane], out int flow) ? flow : 0) ?? 0;
                    }
                    dataInFlowGreaterThan0 = dataInFlowGreaterThan0 + dataForTheHour.Count(hourRow => !hourRow[flowLane].Equals(0));

                    combinedSpeedViolations = combinedSpeedViolations + speedViolationsFlow;
                    combinedExtremeSpeedViolations = combinedExtremeSpeedViolations + extremeSpeedViolationsFlow;
                }
                //Add that combine violations to the object
                var dataQuality = 0;
                if (dataInFlowGreaterThan0 > 0)
                {
                    dataQuality = ((lanesCount * dataForTheHour.Count) / dataInFlowGreaterThan0) * 100;
                }
                violationsForEachHour = violationsForEachHour.PopulateViolationsForEachHour(hour, combinedSpeedViolations, combinedExtremeSpeedViolations, dataQuality);
            }

            return violationsForEachHour;
        }

        private List<List<String>> SplitToTimeGrid(string data)
        {
            var lines = data.Split('\n');
            var headers = lines[0].Split('\t');
            var rows = new List<List<string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split('\t');
                if (columns.Length < headers.Length || columns == null)
                    continue; // Skip incomplete rows

                rows.Add(columns.ToList());
            }
            return rows;
        }

        private async Task<string> LoginToPems()
        {
            using var client = new HttpClient();

            // Set up the request headers
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.Zero
            };
            client.DefaultRequestHeaders.Add("priority", "u=0, i");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not)A;Brand\";v=\"99\", \"Google Chrome\";v=\"127\", \"Chromium\";v=\"127\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
            client.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
            client.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            client.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
            client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");

            // Set up the request content
            var username = Uri.EscapeDataString(configuration["Pems:Username"]);
            var password = Uri.EscapeDataString(configuration["Pems:Password"]);

            var content = new StringContent($"redirect=&username={username}&password={password}&login=Login", Encoding.UTF8, "application/x-www-form-urlencoded");

            // Set up the request
            var request = new HttpRequestMessage(HttpMethod.Post, "https://udot.iteris-pems.com/")
            {
                Content = content,
                Headers =
                {
                    Referrer = new Uri("https://udot.iteris-pems.com/")
                }
            };

            // Send the request
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode(); // Throw an exception if the status code is not successful

            // Get the response content as a string
            var responseBody = await response.Content.ReadAsStringAsync();


            var cookies = response.Headers.GetValues("Set-Cookie");
            var sessionId = string.Empty;

            foreach (var cookie in cookies)
            {
                if (cookie.StartsWith("PHPSESSID"))
                {
                    sessionId = cookie.Split(';')[0]; // Extract the PHPSESSID
                    break;
                }
            }

            return sessionId;
        }
    }
}
