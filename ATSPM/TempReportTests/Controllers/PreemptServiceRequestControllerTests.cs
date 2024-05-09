using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PreemptServiceRequest;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using CsvHelper;
using System.Globalization;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class PreemptServiceRequestControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            PlanService planService = new PlanService();
            PreemptServiceRequestService preemptServiceRequestService = new PreemptServiceRequestService(planService);

            System.DateTime start = new System.DateTime(2023, 4, 17, 12, 0, 0);
            System.DateTime end = new System.DateTime(2023, 4, 17, 14, 0, 0);
            List<IndianaEvent> events = LoadDetectorEventsFromCsv(@"ControllerEvents-Preempt.csv"); // Sampleevents
            List<IndianaEvent> planEvents = events.Where(e => new List<short> { (short)IndianaEnumerations.CoordPatternChange }.Contains(e.EventCode)).ToList(); // Load plan events from CSV
            //List<IndianaEvent> preemptEvents = events.Where(e => new List<int> { 105 }.Contains(e.EventCode)).ToList(); // Load preempt events from CSV

            var options = new PreemptServiceRequestOptions() { LocationIdentifier = "7573", Start = start, End = end };

            //SignalPhase signalPhase = signalPhaseService.GetSignalPhaseData(start, end, true, 0, 15, approach.Object, cycleEvents, planEvents, detectorEvents);
            var viewModel = preemptServiceRequestService.GetChartData(options, planEvents, events);

            // Assert
            Assert.Equal(15, viewModel.PreemptRequests.Count);
            //Assert.NotEmpty(viewModel.ApproachDelayPerVehicleDataPoints);


        }

        private List<IndianaEvent> LoadDetectorEventsFromCsv(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

                List<IndianaEvent> detectorEvents = csv.GetRecords<IndianaEvent>().ToList();
                return detectorEvents;
            }
        }
    }
}