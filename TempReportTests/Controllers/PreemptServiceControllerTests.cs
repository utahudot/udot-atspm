﻿using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PreemptService;
using CsvHelper;
using System.Globalization;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class PreemptServiceControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            PlanService planService = new PlanService();
            PreemptServiceService preemptServiceService = new PreemptServiceService(planService);

            System.DateTime start = new System.DateTime(2023, 4, 17, 12, 0, 0);
            System.DateTime end = new System.DateTime(2023, 4, 17, 14, 0, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"ControllerEvents-Preempt.csv"); // Sampleevents
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV
            List<ControllerEventLog> preemptEvents = events.Where(e => new List<int> { 105 }.Contains(e.EventCode)).ToList(); // Load preempt events from CSV

            var options = new PreemptServiceOptions() { SignalIdentifier = "7573", Start = start, End = end};

            //SignalPhase signalPhase = signalPhaseService.GetSignalPhaseData(start, end, true, 0, 15, approach.Object, cycleEvents, planEvents, detectorEvents);
            var viewModel = preemptServiceService.GetChartData(options, planEvents, preemptEvents);

            // Assert
            Assert.Equal(15, viewModel.PreemptServiceEvents.Count);
            //Assert.NotEmpty(viewModel.ApproachDelayPerVehicleDataPoints);


        }

        private List<ControllerEventLog> LoadDetectorEventsFromCsv(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

                List<ControllerEventLog> detectorEvents = csv.GetRecords<ControllerEventLog>().ToList();
                return detectorEvents;
            }
        }
    }
}