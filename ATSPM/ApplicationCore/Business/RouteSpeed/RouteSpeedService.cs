using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementAggregation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.RouteSpeed
{

    public enum AnalysisPeriod
    {
        AllDay,
        PeekPeriod,
        CustomHour
    }
    public class RouteSpeedService
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private int numRoutes = 14000;

        public RouteSpeedService(IHourlySpeedRepository hourlySpeedRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        public async Task<List<RouteSpeed>> GetRouteSpeedsAsync(RouteSpeedOptions options)

        {
            List<RouteSpeed> routeSpeeds = await hourlySpeedRepository.GetRoutesSpeeds(options);

            return routeSpeeds;
        }

        public async Task AddPemsSpeed(RouteSpeedOptions options)
        {
            HourlySpeed value = new HourlySpeed()
            {
                Date = new DateTime(),
                BinStartTime = new DateTime(),
                RouteId = 1,
                SourceId = 2,
                ConfidenceId = 3,
                Average = 45
            };
            await Task.Run(() => hourlySpeedRepository.AddHourlySpeedAsync(value));
        }

        public async Task AddTestSpeedsPerRoute()
        {
            var speeds = new List<HourlySpeed>();
            for (int i = 0; i < numRoutes; i++)
            {
                speeds.AddRange(GenerateHourlySpeeds(i));
            }
            Console.WriteLine(speeds.Count.ToString());
            await hourlySpeedRepository.AddHourlySpeedsAsync(speeds);
        }

        public List<HourlySpeed> GenerateHourlySpeeds(int routeId)
        {
            var hourlySpeeds = new List<HourlySpeed>();
            var rand = new Random();
            DateTime now = DateTime.UtcNow;
            DateTime firstDayOfLastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1); // First day of last month

            for (int day = 0; day < DateTime.DaysInMonth(firstDayOfLastMonth.Year, firstDayOfLastMonth.Month); day++)
            {
                DateTime currentDate = firstDayOfLastMonth.AddDays(day);

                for (int hour = 0; hour < 24; hour++)
                {
                    DateTime binStartTime = currentDate.AddHours(hour);
                    var hourlySpeed = new HourlySpeed
                    {
                        Date = currentDate,
                        BinStartTime = binStartTime,
                        RouteId = routeId,
                        SourceId = rand.Next(1, 4), // Random source id
                        ConfidenceId = rand.Next(1, 5), // Random confidence id
                        Average = rand.Next(20, 70), // Random average speed
                        FifteenthSpeed = rand.Next(15, 30), // Random 15th percentile speed
                        EightyFifthSpeed = rand.Next(70, 85), // Random 85th percentile speed
                        NinetyFifthSpeed = rand.Next(80, 95), // Random 95th percentile speed
                        NinetyNinthSpeed = rand.Next(90, 100), // Random 99th percentile speed
                        Violation = rand.Next(0, 5), // Random violation count
                        Flow = rand.Next(0, 1000) // Random flow count
                    };

                    hourlySpeeds.Add(hourlySpeed);
                }
            }

            return hourlySpeeds;
        }

        public async Task<HistoricalDTO> GetHistoricalSpeeds(
            int routeId,
            DateOnly startDate,
            DateOnly endDate,
            string daysOfWeek,
            int percentile,
            AnalysisPeriod analysisPeriod)
        {
            var routeSpeeds = new HistoricalDTO
            {
                RouteId = routeId
            };

            var sources = new List<int> { 1, 2, 3 };

            foreach (var sourceId in sources)
            {
                var monthlyAverages = await this.hourlySpeedRepository.GetMonthlyAveragesAsync(routeId, startDate, endDate, daysOfWeek, sourceId);
                var dailyAverages = await this.hourlySpeedRepository.GetDailyAveragesAsync(routeId, startDate, endDate, daysOfWeek, sourceId);

                routeSpeeds.MonthlyHistoricalRouteData.Add(new MonthlyHistoricalRouteData
                {
                    SourceId = sourceId,
                    MonthlyAverages = monthlyAverages
                });

                routeSpeeds.DailyHistoricalRouteData.Add(new DailyHistoricalRouteData
                {
                    SourceId = sourceId,
                    DailyAverages = dailyAverages
                });
            }

            return routeSpeeds;
        }
    }
}
