using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ATSPM.Application.ValueObjects
{
    public class ReportServiceData<Tin, Tout>
    {
        public Tin Options { get; set; }
        public Location Location { get; set; }
        public List<ControllerEventLog> Logs { get; set; }
        public Tout Results { get; set; }
    }

    public class TestDataUtility
    {
        private readonly ILocationRepository _LocationRepo;

        public TestDataUtility(ILocationRepository LocationRepo)
        {
            _LocationRepo = LocationRepo;
        }

        //public FileInfo File { get; set; }

        public FileInfo GenerateTestFile<Tin, Tout>(string Location, Tin options, Tout results, FileInfo logFile)
        {
            var testObject = new ReportServiceData<Tin, Tout>();

            testObject.Options = options;

            testObject.Results = results;

            //testObject.Location = _LocationRepo.GetLatestVersionOfLocation(Location);

            if (logFile.Exists)
            {
                var logs = File.ReadAllLines(logFile.FullName)
                   .Skip(1)
                   .Select(x => x.Split(','))
                   .Select(x => new ControllerEventLog
                   {
                       SignalIdentifier = x[0],
                       Timestamp = DateTime.Parse(x[1]),
                       EventCode = int.Parse(x[2]),
                       EventParam = int.Parse(x[3])
                   }).ToList();

                testObject.Logs = logs;
            }

            var json = JsonSerializer.Serialize(testObject, new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            });

            var result = new FileInfo(Path.Combine(logFile.DirectoryName, $"{Location}-{options.GetType().Name}-ReportTestData.json"));

            File.WriteAllText(result.FullName, json);

            return result;
        }

        public ReportServiceData<Tin, Tout> ReadTestFile<Tin, Tout>(FileInfo file)
        {
            var json = File.ReadAllText(file.FullName);

            var result = JsonSerializer.Deserialize<ReportServiceData<Tin, Tout>>(json);

            return result;
        }
    }
}
