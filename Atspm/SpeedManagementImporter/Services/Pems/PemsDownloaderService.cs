using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeedManagementImporter.Business.Pems;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Utah.Udot.Atspm;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Importer;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementImporter.Services.Pems
{
    public class PemsDownloaderService : IDataDownloader
    {
        private readonly ISegmentRepository segmentRepository;
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly MonthlyAggregationService monthlyAggregationService;
        private readonly IConfiguration configuration; // Change from IConfigurationRoot to IConfiguration
        private readonly ILogger<PemsDownloaderService> logger;
        static readonly int sourceId = 2;
        private string sessionId;

        public PemsDownloaderService(
        ISegmentRepository segmentRepository,
        IHourlySpeedRepository hourlySpeedRepository,
        IConfiguration configuration, // Change here
        ILogger<PemsDownloaderService> logger,
        MonthlyAggregationService monthlyAggregationService)
        {
            this.segmentRepository = segmentRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.configuration = configuration; // Change here
            this.logger = logger;
            this.monthlyAggregationService = monthlyAggregationService;
        }

        public async Task DeleteSegmentData(List<string> providedSegments)
        {
            List<Segment> segments = new List<Segment>();
            foreach (var segmentId in providedSegments)
            {
                var segment = await segmentRepository.LookupAsync(Guid.Parse(segmentId));
                segments.Add(segment);
            }
            var segmentIds = segments.Select(s => s.Id).ToList();
            if (segmentIds.Count == 1)
            {
                await hourlySpeedRepository.DeleteBySegment(segmentIds.FirstOrDefault());
                await monthlyAggregationService.DeleteMonthlyAggregationBySegmentId(segmentIds.FirstOrDefault());
            }
            else
            {
                await hourlySpeedRepository.DeleteBySegments(segmentIds);
                await monthlyAggregationService.DeleteMonthlyAggregationBySegmentIds(segmentIds);
            }
        }

        public async Task Download(DateTime startDate, DateTime endDate, List<string>? providedSegments)
        {
            string apiKey = configuration["Pems:ApiKey"];
            if (apiKey == null)
            {
                logger.LogError("API Key is null, aborting download.");
                return;
            }

            try
            {
                //Have options to have segments ids provided in the beginning
                List<Segment> segments = new List<Segment>();
                if (providedSegments != null && providedSegments.Count > 0)
                {
                    foreach (var segmentId in providedSegments)
                    {
                        var segment = await segmentRepository.LookupAsync(Guid.Parse(segmentId));
                        segments.Add(segment);
                    }
                }
                else
                {
                    segments = segmentRepository.AllSegmentsWithEntity(sourceId);
                }

                var start = DateTime.Now;
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    logger.LogInformation($"Starting download for date {date:yyyy-MM-dd}");
                    List<Task> routeTasks = new List<Task>();

                    foreach (var segment in segments)
                    {
                        routeTasks.Add(ProcessSegmentForDate(segment, date, apiKey));

                        // Limit parallelism
                        if (routeTasks.Count >= 40)
                        {
                            await Task.WhenAny(routeTasks);
                            routeTasks.RemoveAll(t => t.IsCompleted);
                        }
                    }

                    await Task.WhenAll(routeTasks);
                    logger.LogInformation($"Finished processing all routes for date {date:yyyy-MM-dd}");
                }
                var end = DateTime.Now;
                var runTime = end - start;
                //Log the minutes and seconds taken to download the data
                logger.LogInformation($"PEMS download process completed in {runTime.Minutes} minutes and {runTime.Seconds} seconds.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during the PEMs download process.");
            }
        }

        // Process each segment for a specific date
        private Task ProcessSegmentForDate(Segment segment, DateTime date, string apiKey)
        {
            return Task.Run(async () =>
            {
                var segmentStatistics = new List<DayStatistics>();
                if (segment.Entities != null)
                {
                    foreach (var entity in segment.Entities)
                    {
                        try
                        {
                            segmentStatistics.Add(await GetStatisticsForDay(date, entity.EntityId, segment.SpeedLimit, true));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Error processing Station: {entity.EntityId} on {date:yyyy-MM-dd}");
                        }
                    }
                    var speeds = GetSegmentSpeeds(segment, segmentStatistics, date);
                    try
                    {
                        await hourlySpeedRepository.AddHourlySpeedsAsync(speeds);
                        logger.LogInformation($"Speeds added for Segment: {segment.Id}-{segment.Name} ({speeds.Count} records) for {date:yyyy-MM-dd}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error error uploading data for Segement: {segment.Id}-{segment.Name} for {date:yyyy-MM-dd}");
                    }
                }
            });
        }

        private List<HourlySpeed> GetSegmentSpeeds(Segment segment, List<DayStatistics> segmentStatistics, DateTime date)
        {
            var hourlySpeeds = new List<HourlySpeed>();

            // Ensure segmentStatistics has data to process
            if (segmentStatistics == null || !segmentStatistics.Any())
            {
                throw new InvalidOperationException("segmentStatistics is null or empty.");
            }

            for (int hour = 0; hour < 24; hour++)
            {
                try
                {
                    var hourlyStatistics = segmentStatistics
                        .SelectMany(s => s.HourlyStatistics)
                        .Where(s => s.Hour == hour && s.SourceDataAnalyzed)
                        .ToList();

                    if (!hourlyStatistics.Any())
                    {
                        hourlySpeeds.Add(new HourlySpeed
                        {
                            Date = date.Date,
                            BinStartTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0),
                            SegmentId = segment.Id,
                            SourceId = sourceId,  // Ensure sourceId is set correctly in the broader scope
                            PercentObserved = 0,
                            Flow = 0,
                            Violation = null,
                            ExtremeViolation = null,
                            Average = 0,
                            FifteenthSpeed = null,
                            EightyFifthSpeed = null,
                            NinetyFifthSpeed = null,
                            NinetyNinthSpeed = null,
                            MinSpeed = null,
                            MaxSpeed = null,
                            SourceDataAnalyzed = false
                        });
                        // Skip processing for this hour if there is no data
                        logger.LogWarning($"No data available for hour {hour} for segment {segment.Id}");
                        continue;
                    }

                    var combinedSpeeds = hourlyStatistics
                        .SelectMany(s => s.WeightedSpeeds)
                        .ToList();

                    var combinedSpeedFlowMismatches = hourlyStatistics.Sum(s => s.SpeedFlowMismatches);
                    var totalBins = hourlyStatistics.Sum(s => s.TotalBins);

                    // Check for division by zero
                    var percentObserved = totalBins > 0 ? 100D - (combinedSpeedFlowMismatches / totalBins * 100D) : 0;
                    var summedFlow = hourlyStatistics.Any() ? hourlyStatistics.Average(s => s.Flow) : 0;
                    //var summedFlowSpeedProduct = hourlyStatistics.Any() ? hourlyStatistics.Sum(s => s.TotalFlowSpeedProduct) : 0;

                    // Calculate weighted average only if summedFlow is greater than 0
                    //double? weightedAverage = summedFlow > 0 ? summedFlowSpeedProduct / summedFlow : null;

                    // Safeguard for speed calculations
                    // Round to the first decimal place to avoid floating point errors

                    var averageSpeed = combinedSpeeds.Count > 0 ? Math.Round(combinedSpeeds.Average(), 1) : 0;
                    double? fifteenthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 15), 1) : null;
                    double? eightyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 85), 1) : null;
                    double? ninetyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 95), 1) : null;
                    double? ninetyNinthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 99), 1) : null;
                    double? minspeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Min() : null;
                    double? maxspeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Max() : null;



                    hourlySpeeds.Add(new HourlySpeed
                    {
                        Date = date.Date,
                        BinStartTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0),
                        SegmentId = segment.Id,
                        SourceId = sourceId,  // Ensure sourceId is set correctly in the broader scope
                        PercentObserved = percentObserved,
                        Flow = (long)summedFlow,
                        Violation = (long?)hourlyStatistics?.Average(s => s.Violations),
                        ExtremeViolation = (long?)hourlyStatistics?.Average(s => s.ExtremeViolations),
                        Average = averageSpeed,
                        FifteenthSpeed = fifteenthPercentile,
                        EightyFifthSpeed = eightyFifthPercentile,
                        NinetyFifthSpeed = ninetyFifthPercentile,
                        NinetyNinthSpeed = ninetyNinthPercentile,
                        MinSpeed = minspeed,
                        MaxSpeed = maxspeed,
                        SourceDataAnalyzed = true
                    });
                }
                catch (Exception ex)
                {
                    // Log the error and continue to the next hour
                    logger.LogError(ex, $"Error processing hour {hour} for segment {segment.Id}");
                    throw; // You can decide whether to rethrow or handle this more gracefully
                }
            }

            return hourlySpeeds;
        }





        private static int? GetNullSafeAverage(List<double> statisticsByRoute)
        {
            if (statisticsByRoute == null || !statisticsByRoute.Any())
                return null;
            double? eightyFifthSpeed = statisticsByRoute.Average();
            return eightyFifthSpeed != null ? (int)Math.Round(eightyFifthSpeed.Value) : null;
        }


        private static async Task<List<StationMeasurement>> GetPemsStatisticsAsync(string url, int retry = 0, ILogger logger = null)
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
                        if (json["measurements"].Count() == 0)
                        {
                            return null;
                        }
                        var rootObject = JsonConvert.DeserializeObject<RootObjectSpeed>(jsonResponse);
                        return rootObject.measurements.station;
                    }
                    else
                    {
                        logger?.LogWarning($"HTTP Error (Stats): {response.StatusCode} - {response.ReasonPhrase} | Retry {retry}");
                        if (retry < 10)
                        {
                            await Task.Delay(2000); // Introducing a delay before retrying
                            return await GetPemsStatisticsAsync(url, retry + 1, logger);
                        }
                        return null;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    logger?.LogError(httpEx, $"HttpRequestException occurred | Retry {retry}");
                    if (retry < 10)
                    {
                        await Task.Delay(2000); // Delay before retrying
                        return await GetPemsStatisticsAsync(url, retry + 1, logger);
                    }
                    return null;
                }
                catch (TaskCanceledException timeoutEx)
                {
                    logger?.LogError(timeoutEx, $"Timeout error occurred | Retry {retry}");
                    if (retry < 10)
                    {
                        await Task.Delay(2000); // Delay before retrying
                        return await GetPemsStatisticsAsync(url, retry + 1, logger);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    logger?.LogCritical(ex, $"General error occurred");
                    return null;
                }
            }
        }


        private async Task<DayStatistics> GetStatisticsForDay(DateTime originalDateTime, long stationId, long speedLimit, bool freeway = true, int retry = 0)
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
                    AllowAutoRedirect = true,
                    UseCookies = true,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var httpClient = new HttpClient(handler))
                {
                    int desiredHour = 0; // Midnight
                    int desiredMinute = 0; // Zero minutes

                    // Construct the start date and time using originalDateTime with desired hour and minute
                    DateTime startDate = new DateTime(
                        originalDateTime.Year,
                        originalDateTime.Month,
                        originalDateTime.Day,
                        desiredHour,
                        desiredMinute,
                        0 // Seconds
                    );


                    // Format and encode the start date
                    string formattedStartDate = startDate.ToString("MM/dd/yyyy HH:mm:ss");
                    string urlEncodedStartDate = HttpUtility.UrlEncode(formattedStartDate);

                    // Convert to Unix time (UTC-based)
                    long unixStartTime = new DateTimeOffset(startDate, TimeSpan.Zero).ToUnixTimeSeconds();

                    // Set end time to 23:59:59 of the same day (end of the day)
                    DateTime endDate = new DateTime(
                        originalDateTime.Year,
                        originalDateTime.Month,
                        originalDateTime.Day,
                        23, // Hour set to 23
                        59, // Minute set to 59
                        59  // Seconds set to 59
                    );


                    // Format and encode the end date
                    string formattedEndDate = endDate.ToString("MM/dd/yyyy HH:mm:ss");
                    string urlEncodedEndDate = HttpUtility.UrlEncode(formattedEndDate);
                    // Convert to Unix time (UTC-based)
                    long unixEndTime = new DateTimeOffset(endDate, TimeSpan.Zero).ToUnixTimeSeconds();

                    // Your download URL
                    var downloadDataUrl = $"https://udot.iteris-pems.com/?report_form=1&dnode=VDS&content=detector_health&tab=dh_raw&export=text&station_id={stationId}&s_time_id={unixStartTime}&s_time_id_f={urlEncodedStartDate}&e_time_id={unixEndTime}&e_time_id_f={urlEncodedEndDate}&lanes={stationId}-0&q=speed_used&q2=flow&gn=sec";

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

                    // Call the endpoint
                    var TwentySecondResults = await httpClient.GetAsync(downloadDataUrl);

                    if (!TwentySecondResults.IsSuccessStatusCode)
                    {
                        logger?.LogWarning($"HTTP Error: {TwentySecondResults.StatusCode} - {TwentySecondResults.ReasonPhrase}. Station: {stationId} Retry {retry}");
                        if (retry < 3)
                        {
                            await Task.Delay(2000); // Introduce a delay before retrying
                            return await GetStatisticsForDay(originalDateTime, stationId, speedLimit, freeway, retry + 1);
                        }
                        return new DayStatistics();
                    }

                    var TwentySecondSpeedString = await TwentySecondResults.Content.ReadAsStringAsync();
                    List<List<string>> rowsOfColumns = SplitToTimeGrid(TwentySecondSpeedString);
                    if (!rowsOfColumns.IsNullOrEmpty())
                    {
                        DayStatistics dayStatistics = SetDayStatistics(speedLimit, freeway, rowsOfColumns, originalDateTime, stationId);
                        return dayStatistics;
                    }
                    return new DayStatistics();
                }
            }
            catch (HttpRequestException httpEx)
            {
                logger?.LogError(httpEx, $"HttpRequestException occurred for station {stationId}. Retry {retry}");
                if (retry < 3)
                {
                    await Task.Delay(2000); // Delay before retrying
                    return await GetStatisticsForDay(originalDateTime, stationId, speedLimit, freeway, retry + 1);
                }
                return new DayStatistics();
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, $"Critical error while fetching violations for station {stationId}. Retry {retry}");
                if (retry < 3)
                {
                    await Task.Delay(2000); // Delay before retrying
                    this.sessionId = await LoginToPems(); // Re-login and retry
                    return await GetStatisticsForDay(originalDateTime, stationId, speedLimit, freeway, retry + 1);
                }

                logger?.LogError($"Max retries reached for station {stationId}. Returning empty result.");
                return new DayStatistics();
            }
        }


        private DayStatistics SetDayStatistics(long speedLimit, bool freeway, List<List<string>> laneData, DateTime date, long stationId)
        {
            var dayStatistics = new DayStatistics
            {
                SpeedLimit = speedLimit,
                Date = date
            };

            try
            {
                if (laneData == null || laneData.Count == 0)
                {
                    logger?.LogWarning("No rows of data available to process violations.");
                    return dayStatistics;
                }

                // Validate laneData columns count
                if (laneData[0].Count < 3)
                {
                    logger?.LogError("Invalid data format. Each row must have at least a timestamp and data for lanes.");
                    return dayStatistics;
                }

                var lanesCount = (laneData[0].Count - 1) / 2;
                var startOfSpeedLocation = 1;
                var startOfFlowLocation = lanesCount + 1;

                for (var hour = 0; hour <= 23; hour++)
                {
                    var dataForTheHour = laneData
                        .Where(row => DateTime.TryParse(row[0], out var dateTime) && dateTime.Hour == hour && dateTime.Day == date.Day)
                        .ToList();
                    if (dataForTheHour == null || dataForTheHour.Count == 0)
                    {
                        dayStatistics.HourlyStatistics.Add(new HourlyStatistics
                        {
                            Hour = hour,
                            Violations = 0,
                            ExtremeViolations = 0,
                            Flow = 0,
                            WeightedSpeeds = new List<double>(),
                            SpeedFlowMismatches = 0,
                            TotalBins = 0,
                            SourceDataAnalyzed = false
                            //TotalFlowSpeedProduct = combinedFlowSpeedProduct
                        });
                        logger?.LogWarning($"Station - {stationId} Skipping hour {hour} due to invalid data.");
                        continue;
                    }

                    int? combinedSpeedViolations = null;
                    int? combinedExtremeSpeedViolations = null;
                    var combinedFlow = 0;
                    var combinedSpeedFlowMismatches = 0;
                    //var combinedFlowSpeedProduct = 0D;
                    var combinedWeightedSpeeds = new List<double>();

                    for (var lane = 0; lane < lanesCount; lane++)
                    {
                        int speedColumnIndex = startOfSpeedLocation + lane;
                        int flowColumnIndex = startOfFlowLocation + lane;

                        if (speedColumnIndex >= laneData[0].Count || flowColumnIndex >= laneData[0].Count)
                        {
                            logger?.LogWarning($"Station - {stationId} Skipping lane {lane} for hour - {hour}  due to invalid data.");
                            continue;
                        }
                        var laneDataForHour = new List<ParsedLaneDataForHour>();
                        foreach (var row in dataForTheHour)
                        {
                            if (DateTime.TryParse(row[0], out var dateTime) && dateTime.Hour == hour && dateTime.Day == date.Day)
                            {
                                laneDataForHour.Add(new ParsedLaneDataForHour
                                {
                                    DateTime = dateTime,
                                    Speed = double.TryParse(row[speedColumnIndex], out double speed) ? speed : (double?)null,
                                    Flow = int.TryParse(row[flowColumnIndex], out int flow) ? flow : (int?)null
                                });
                            }
                        }

                        var laneFlow = laneDataForHour
                            .Sum(i => i.Flow ?? 0);
                        combinedFlow += laneFlow;

                        int? extremeSpeedViolations = null;
                        int? speedViolations = null;
                        if (speedLimit > 0)
                        {
                            if (combinedExtremeSpeedViolations == null)
                            {
                                combinedExtremeSpeedViolations = 0;
                            }
                            if (combinedSpeedViolations == null)
                            {
                                combinedSpeedViolations = 0;
                            }
                            speedViolations = laneDataForHour
                                .Where(i => i.Speed.HasValue && i.Speed >= (speedLimit + 2))
                                .Sum(i => i.Flow ?? 0);

                            if (freeway)
                            {
                                extremeSpeedViolations = laneDataForHour
                                    .Where(i => i.Speed.HasValue && i.Speed >= (speedLimit + 10))
                                    .Sum(i => i.Flow ?? 0);
                            }
                            else
                            {
                                extremeSpeedViolations = laneDataForHour
                                    .Where(i => i.Speed.HasValue && i.Speed >= (speedLimit + 7))
                                    .Sum(i => i.Flow ?? 0);
                            }
                            combinedSpeedViolations += speedViolations.HasValue ? speedViolations.Value : 0;
                            combinedExtremeSpeedViolations += extremeSpeedViolations.HasValue ? extremeSpeedViolations.Value : 0;
                        }

                        combinedSpeedFlowMismatches += laneDataForHour.Count(hourRow => hourRow.Flow.HasValue && hourRow.Flow > 0 && !hourRow.Speed.HasValue);


                        // Add valid speeds to the list
                        //combinedWeightedSpeeds.AddRange(dataForTheHour.Where(i => double.TryParse(i[speedColumnIndex], out _)).Select(i => double.Parse(i[speedColumnIndex])));
                        foreach (var row in laneDataForHour)
                        {
                            if (row.Speed.HasValue && row.Flow.HasValue)
                            {
                                // Add the speed multiple times based on the flow
                                for (int i = 0; i < row.Flow; i++)
                                {
                                    combinedWeightedSpeeds.Add(row.Speed.Value);
                                }
                            }
                        }

                        // Calculate combined flow-speed product
                        //combinedFlowSpeedProduct += dataForTheHour
                        //    .Where(i => double.TryParse(i[speedColumnIndex], out _) && double.TryParse(i[flowColumnIndex], out _))
                        //    .Sum(i => double.Parse(i[speedColumnIndex]) * double.Parse(i[flowColumnIndex]));
                    }

                    dayStatistics.HourlyStatistics.Add(new HourlyStatistics
                    {
                        Hour = hour,
                        Violations = combinedSpeedViolations,
                        ExtremeViolations = combinedExtremeSpeedViolations,
                        Flow = combinedFlow,
                        WeightedSpeeds = combinedWeightedSpeeds,
                        SpeedFlowMismatches = combinedSpeedFlowMismatches,
                        TotalBins = lanesCount * (dataForTheHour?.Count ?? 0),
                        SourceDataAnalyzed = true
                        //TotalFlowSpeedProduct = combinedFlowSpeedProduct
                    });
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while processing violations.");
            }

            return dayStatistics;
        }


        private List<List<string>> SplitToTimeGrid(string data)
        {
            var rows = new List<List<string>>();
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                {
                    //logger?.LogWarning("Input data is empty or null.");
                    return rows;
                }

                var lines = data.Split('\n');
                var headers = lines[0].Split('\t');

                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i]?.Split('\t');
                    if (columns == null || columns.Length < headers.Length)
                    {
                        //logger?.LogWarning($"Skipping incomplete or malformed row: {lines[i]}");
                        continue; // Skip incomplete or malformed rows
                    }

                    rows.Add(columns.ToList());
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while splitting the data into a time grid.");
            }

            return rows;
        }



        private async Task<string> LoginToPems(ILogger logger = null)
        {
            using var client = new HttpClient();

            try
            {
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

                // Check if response is successful
                if (!response.IsSuccessStatusCode)
                {
                    logger?.LogWarning($"Failed login attempt. HTTP Status: {response.StatusCode} - {response.ReasonPhrase}");
                    return string.Empty;
                }

                // Get the response content as a string
                var responseBody = await response.Content.ReadAsStringAsync();

                // Check for session ID in the cookies
                if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    logger?.LogWarning("No Set-Cookie header found in the response.");
                    return string.Empty;
                }

                var sessionId = string.Empty;

                foreach (var cookie in cookies)
                {
                    if (cookie.StartsWith("PHPSESSID"))
                    {
                        sessionId = cookie.Split(';')[0]; // Extract the PHPSESSID
                        break;
                    }
                }

                if (string.IsNullOrEmpty(sessionId))
                {
                    logger?.LogWarning("Failed to extract PHPSESSID from cookies.");
                }

                return sessionId;
            }
            catch (HttpRequestException httpEx)
            {
                logger?.LogError(httpEx, "HTTP request error occurred during PEMS login.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "An unexpected error occurred during PEMS login.");
                return string.Empty;
            }
        }

    }

    public class ParsedLaneDataForHour
    {
        public DateTime DateTime { get; set; }
        public double? Speed { get; set; }
        public int? Flow { get; set; }
    }

}
