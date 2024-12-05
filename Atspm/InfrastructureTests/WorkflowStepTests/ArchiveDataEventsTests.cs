using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.ATSPM.InfrastructureTests.WorkflowStepTests
{
    public class ArchiveDataEventsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public ArchiveDataEventsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(ArchiveDataEventsTests), "LocationIdentifer grouping")]
        public async void ArchiveDataEventsTestsLocationIdentiferGrouping()
        {
            var input = Enumerable.Range(1, 10)
                .Select(s =>
                {
                    {
                        return Tuple.Create<Device, EventLogModelBase>(new Device() { Id = s }, new IndianaEvent()
                        {
                            LocationIdentifier = (s % 2 == 0) ? "1002" : "1001",
                            EventCode = Convert.ToInt16(s),
                            EventParam = Convert.ToInt16(s),
                            Timestamp = DateTime.Now.AddSeconds(s)
                        });
                    }
                }).ToArray();

            var sut = new ArchiveDataEvents();

            var result = new List<CompressedEventLogBase>();

            await foreach (var i in sut.Execute(input))
            {
                result.Add(i);

                _output.WriteLine($"{i.LocationIdentifier} - {i.DeviceId} - {i.ArchiveDate} - {i.DataType.Name} - {i.Data.Count()}");
            }

            var actual = result.GroupBy(g => g.LocationIdentifier).Select(s => s.Count()).Average();

            var expected = 5;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(ArchiveDataEventsTests), "TimeStamp grouping")]
        public async void ArchiveDataEventsTestsTimeStampGrouping()
        {
            var time1 = DateTime.Now;
            var time2 = time1.AddDays(5);
            
            var input = Enumerable.Range(1, 10)
                .Select(s =>
                {
                    {
                        return Tuple.Create<Device, EventLogModelBase>(new Device() { Id = s }, new IndianaEvent()
                        {
                            LocationIdentifier = "1001",
                            EventCode = Convert.ToInt16(s),
                            EventParam = Convert.ToInt16(s),
                            Timestamp = (s % 2 == 0) ? time1 : time2
                        });
                    }
                }).ToArray();

            var sut = new ArchiveDataEvents();

            var result = new List<CompressedEventLogBase>();

            await foreach (var i in sut.Execute(input))
            {
                result.Add(i);

                _output.WriteLine($"{i.LocationIdentifier} - {i.DeviceId} - {i.ArchiveDate} - {i.DataType.Name} - {i.Data.Count()}");
            }

            var actual = result.GroupBy(g => g.ArchiveDate).Select(s => s.Count()).Average();

            var expected = 5;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(ArchiveDataEventsTests), "DeviceId grouping")]
        public async void ArchiveDataEventsTestsDeviceIdGrouping()
        {
            var time = DateTime.Now;

            var input = Enumerable.Range(1, 10)
                .Select(s =>
                {
                    {
                        return Tuple.Create<Device, EventLogModelBase>(new Device() { Id = (s % 2 == 0) ? 1 : 2 }, new IndianaEvent()
                        {
                            LocationIdentifier = "1001",
                            EventCode = Convert.ToInt16(s),
                            EventParam = Convert.ToInt16(s),
                            Timestamp = time
                        });
                    }
                }).ToArray();

            var sut = new ArchiveDataEvents();

            var result = new List<CompressedEventLogBase>();

            await foreach (var i in sut.Execute(input))
            {
                result.Add(i);

                _output.WriteLine($"{i.LocationIdentifier} - {i.DeviceId} - {i.ArchiveDate} - {i.DataType.Name} - {i.Data.Count()}");
            }

            var actual = result.GroupBy(g => g.DeviceId).Select(s => s.Count()).Average();

            var expected = 1;

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(ArchiveDataEventsTests), "DataType grouping")]
        public async void ArchiveDataEventsTestsDataTypeGrouping()
        {
            var time = DateTime.Now;

            var input = Enumerable.Range(1, 10)
                .Select(s =>
                {
                    {
                        return Tuple.Create<Device, EventLogModelBase>(new Device() { Id = s }, (s % 2 == 0) ? 
                            new IndianaEvent()
                            {
                                LocationIdentifier = "1001",
                                EventCode = Convert.ToInt16(s),
                                EventParam = Convert.ToInt16(s),
                                Timestamp = time
                            } :
                            new SpeedEvent()
                            {
                                LocationIdentifier = "1001",
                                Timestamp = time,
                                DetectorId = s.ToString()
                            });
                    }
                }).ToArray();

            var sut = new ArchiveDataEvents();

            var result = new List<CompressedEventLogBase>();

            await foreach (var i in sut.Execute(input))
            {
                result.Add(i);

                _output.WriteLine($"{i.LocationIdentifier} - {i.DeviceId} - {i.ArchiveDate} - {i.DataType.Name} - {i.Data.Count()}");
            }

            var actual = result.GroupBy(g => g.DataType).Select(s => s.Count()).Average();

            var expected = 5;

            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
        }
    }
}
