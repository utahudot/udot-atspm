using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.Atspm.Infrastructure.Services.TurningMovementCounts.Decoders.EconoliteVision;
using Utah.Udot.Atspm.ReportApi.Controllers;
using Utah.Udot.Atspm.ReportApi.ReportServices;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Services;

namespace ReportApiTests;

// HTTP endpoint -> real report service -> real decoder -> simulated camera HTTP -> result builder.
// Repositories supply isolated configuration; no physical camera or production database is required.
public class TmcEndToEndTests
{
    private sealed class Camera : HttpMessageHandler
    {
        public int Calls, Current, Maximum;
        public string OfflineIndex = "";
        public string ZoneName;
        public string StatisticsJson;
        public string BadRequestIndex = "";
        public List<string> Paths = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var n=Interlocked.Increment(ref Current); lock(Paths){ Maximum=Math.Max(Maximum,n); Paths.Add(request.RequestUri.ToString()); }
            try {
                await Task.Delay(10,token);
                var path=request.RequestUri.AbsolutePath;
                if(path.EndsWith("/cameras")) return Json("{\"cameras\":[{\"index\":\"1\",\"deviceId\":\"523648495\"}]}");
                var index=path.Split('/')[4];
                if(index==OfflineIndex) throw new HttpRequestException("camera offline");
                if(path.EndsWith("device-info")) return Json("{\"name\":\"O420 02 NB 05 LT\"}");
                Interlocked.Increment(ref Calls);
                if(index==BadRequestIndex) return new(HttpStatusCode.BadRequest){Content=new StringContent("invalid camera date")};
                if(StatisticsJson != null) return Json(StatisticsJson);
                return Json(JsonSerializer.Serialize(new { statistics = new[] { new { zoneId=int.Parse(index), zoneName=ZoneName ?? "N2"+index+"-1", time="2026-09-25T17:15:00Z", volume=7, throughCount=4, leftTurnCount=2, rightTurnCount=1 } } }));
            } finally { Interlocked.Decrement(ref Current); }
        }
        static HttpResponseMessage Json(string body)=>new(HttpStatusCode.OK){Content=new StringContent(body)};
    }
    private sealed class Fixture : IDisposable
    {
        public TestServer Server; public HttpClient Client; public Camera Camera=new(); public List<Device> Devices;
        public Fixture(int count=1,bool? enabled=true, IEventLogRepository statistics=null, List<IndianaEvent> indiana=null, Action<IServiceCollection> configure=null, bool? laneReconciliation=null)
        {
            var location=new Location { Id=1,LocationIdentifier="1",PrimaryName="Test",Latitude=33.35,Longitude=-111.79 };
            Devices=Enumerable.Range(1,count).Select(i=>new Device { Id=i,LocationId=1,Location=location,DeviceIdentifier=i.ToString(),Ipaddress="127.0.0.1",DeviceType=DeviceTypes.FIRCamera,DeviceStatus=DeviceStatus.Active,
                DeviceProperties=new(){["TmcDecoder"]=nameof(VisionCameraAPI)}, DeviceConfiguration=new(){Port=8080,Path="/api/v1/cameras",Product=new(){Manufacturer="Econolite",Model="Vision"}} }).ToList();
            var repo=new Mock<IDeviceRepository>(); repo.Setup(r=>r.GetList()).Returns(()=>Devices.AsQueryable());
            var locations=new Mock<ILocationRepository>(); locations.Setup(r=>r.GetLatestVersionOfLocation("1",It.IsAny<DateTime>())).Returns(location);
            var events=new Mock<IIndianaEventLogRepository>(); events.Setup(r=>r.GetEventsBetweenDates(It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<DateTime>())).Returns(indiana ?? new List<IndianaEvent>());
            Server=new TestServer(new WebHostBuilder().ConfigureAppConfiguration(c=>c.AddInMemoryCollection(enabled.HasValue
                ? new Dictionary<string,string>{{"Features:TmcDeviceSources",enabled.Value.ToString()}}
                : new Dictionary<string,string>()).AddInMemoryCollection(laneReconciliation.HasValue
                    ? new Dictionary<string,string>{{"Features:LaneReconciliation", laneReconciliation.Value.ToString()}}
                    : new Dictionary<string,string>()))
                .ConfigureServices(s=>{
                    s.AddLogging(); s.AddControllers().AddApplicationPart(typeof(TurningMovementCountsController).Assembly);
                    s.AddApiVersioning().AddMvc(); s.AddTmcCountSources();
                    s.AddHttpClient("TmcCamera").ConfigurePrimaryHttpMessageHandler(() => Camera);
                    if (statistics != null) s.AddSingleton(statistics);
                    s.AddSingleton(repo.Object); s.AddSingleton(locations.Object); s.AddSingleton(events.Object);
                    s.AddScoped<TmcCountSourceResolver>(); s.AddScoped<TurningMovementCountsService>(); s.AddScoped<PlanService>();
                    s.AddScoped<IReportService<TurningMovementCountsOptions,TurningMovementCountsResult>,TurningMovementCountReportService>();
                    configure?.Invoke(s);
                }).Configure(app=>{app.UseRouting();app.UseEndpoints(e=>e.MapControllers());}));
            Client=Server.CreateClient();
        }
        public Task<HttpResponseMessage> Report(params int[] ids)=>Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData",new {
            source="devices",deviceIds=ids,locationIdentifier="1",start="2026-09-25T10:00:00",end="2026-09-25T11:00:00",binSize=15 });
        public void Dispose(){Client.Dispose();Server.Dispose();}
    }
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task LaneEvidenceIsIndependentlyDefaultOff(bool? enabled)
    {
        using var f = new Fixture(laneReconciliation: enabled);
        using var response = await LaneReport(f);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(f.Camera.Paths);
    }

    private static Task<HttpResponseMessage> LaneReport(Fixture f, string decoder = "VisionCameraAPI") =>
        f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData", new {
            source="devices", decoder, deviceIds=f.Devices.Select(d => d.Id).ToArray(), locationIdentifier="1", reconcileLanes=true,
            start="2026-09-25T10:00:00", end="2026-09-25T11:00:00", binSize=15 });

    [Fact]
    public async Task LaneAliasesUseOneInventoryOwnerWithoutBlockingWarning()
    {
        using var f = new Fixture(count: 2, laneReconciliation: true);
        f.Devices[1].DeviceIdentifier = "523648495";
        using var response = await LaneReport(f);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(new[] { 1 }, json.RootElement.GetProperty("sourceDeviceIds").EnumerateArray().Select(v => v.GetInt32()));
        Assert.Single(json.RootElement.GetProperty("zoneEvidence").EnumerateArray());
        Assert.Empty(json.RootElement.GetProperty("warnings").EnumerateArray());
        Assert.Equal(1, f.Camera.Calls);
    }

    [Fact]
    public async Task UnnamedLaneZonesRemainSeparateWithSourceIds()
    {
        using var f = new Fixture(laneReconciliation: true);
        f.Camera.StatisticsJson = """
            {"statistics":[
              {"zoneId":101,"zoneName":null,"time":"2026-09-25T17:15:00Z","throughCount":4},
              {"zoneId":102,"zoneName":null,"time":"2026-09-25T17:15:00Z","throughCount":9}]}
            """;
        using var response = await LaneReport(f);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var rows = json.RootElement.GetProperty("zoneEvidence").EnumerateArray().ToArray();
        Assert.Equal(new long[] {101, 102}, rows.Select(r => r.GetProperty("zoneId").GetInt64()));
        Assert.Equal(new long[] {4, 9}, rows.Select(r => r.GetProperty("through").GetInt64()));
        Assert.Empty(json.RootElement.GetProperty("warnings").EnumerateArray());
    }

    [Fact]
    public async Task NegativeLaneCountsIdentifyTheCamera()
    {
        using var f = new Fixture(laneReconciliation: true);
        f.Devices[0].DeviceIdentifier = "523648495";
        f.Camera.StatisticsJson = """
            {"statistics":[{"zoneId":101,"zoneName":"Lane","time":"2026-09-25T17:15:00Z","throughCount":-1}]}
            """;
        using var response = await LaneReport(f);
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Contains("Camera 523648495 (device 1): negative movement counts", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChartAndLaneEvidenceShareExclusiveTimeRange(bool reconcileLanes)
    {
        using var f = new Fixture(laneReconciliation: true);
        f.Camera.StatisticsJson = """
            {"statistics":[
              {"zoneId":1,"zoneName":"NB","time":"2026-09-25T16:45:00Z","throughCount":100},
              {"zoneId":1,"zoneName":"NB","time":"2026-09-25T17:00:00Z","throughCount":4},
              {"zoneId":1,"zoneName":"NB","time":"2026-09-25T18:00:00Z","throughCount":200}]}
            """;
        using var response = reconcileLanes ? await LaneReport(f) : await f.Report(1);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(4, reconcileLanes
            ? json.RootElement.GetProperty("zoneEvidence")[0].GetProperty("through").GetInt32()
            : json.RootElement.GetProperty("charts").EnumerateArray().Sum(r => r.GetProperty("totalVolume").GetInt32()));
    }

    [Theory]
    [InlineData("A231-1")]
    [InlineData("RR5-1")]
    [InlineData("Any town's lane name")]
    public async Task LaneEvidenceUsesUnfilteredZonesWithoutApproachConfiguration(string zone)
    {
        using var f = new Fixture(enabled: false, laneReconciliation: true);
        f.Camera.ZoneName = zone;
        using var response = await LaneReport(f);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var evidence = json.RootElement.GetProperty("zoneEvidence")[0];
        Assert.Equal(zone, evidence.GetProperty("zoneName").GetString());
        Assert.Equal(4, evidence.GetProperty("through").GetInt64());
        Assert.Equal(2, evidence.GetProperty("left").GetInt64());
        Assert.Equal(1, evidence.GetProperty("right").GetInt64());
        Assert.Equal(1, evidence.GetProperty("deviceId").GetInt32());
        Assert.Empty(json.RootElement.GetProperty("charts").EnumerateArray());
        Assert.Contains(f.Camera.Paths, p => p.Contains("start-time=2026-09-25T17:00:00Z"));
    }

    [Fact]
    public async Task DecoderWithoutLaneCapabilityIsRejectedBeforeCameraRead()
    {
        using var f = new Fixture(laneReconciliation: true);
        f.Devices[0].DeviceProperties["TmcDecoder"] = nameof(AutomaticallyDiscoveredDecoder);
        using var response = await LaneReport(f, nameof(AutomaticallyDiscoveredDecoder));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(f.Camera.Paths);
    }

    public sealed class FutureLaneDecoder : ITurningMovementCountDecoder
    {
        public int MinimumBinMinutes => 15;
        public bool SupportsLaneReconciliation => true;
        public bool CanDecode(Device device) => true;
        public Task<TmcDecodeResult> DecodeAsync(TmcDecodeRequest request, CancellationToken token) =>
            Task.FromResult(new TmcDecodeResult(Array.Empty<MovementCount>(), Array.Empty<string>()) {
                ZoneEvidence = new[] { new TmcZoneEvidence(request.Device.Id, "Vendor-independent zone", 12, 3, 4, 1) }
            });
    }

    [Fact]
    public async Task FutureDecoderCanProvideEvidenceWithoutControllerOrReportServiceChanges()
    {
        using var f = new Fixture(laneReconciliation: true);
        f.Devices[0].DeviceProperties["TmcDecoder"] = nameof(FutureLaneDecoder);
        using var response = await LaneReport(f, nameof(FutureLaneDecoder));
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Vendor-independent zone", result.RootElement.GetProperty("zoneEvidence")[0].GetProperty("zoneName").GetString());
        Assert.Empty(f.Camera.Paths);
    }

    private sealed class FutureCountSource : ITurningMovementCountSource
    {
        public Task<TmcCountSourceResult> ReadAsync(TmcCountSourceRequest request, CancellationToken token) =>
            Task.FromResult(new TmcCountSourceResult("Future source", new[]{
                new MovementCount(DirectionTypes.WB,MovementTypes.T,LaneTypes.V,1,request.Options.Start,15,8)
            },Array.Empty<string>()));
    }
    // Discovered from this loaded assembly by the same helper used for logging decoders.
    // No explicit registration is made for this test decoder.
    public sealed class AutomaticallyDiscoveredDecoder : ITurningMovementCountDecoder
    {
        public int MinimumBinMinutes => 15;
        public bool CanDecode(Device device) => true;
        public Task<TmcDecodeResult> DecodeAsync(TmcDecodeRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new TmcDecodeResult(new[] {
                new MovementCount(DirectionTypes.WB, MovementTypes.T, LaneTypes.V, 1, request.Options.Start, 15, 8)
            }, Array.Empty<string>()));
    }

    [Fact] public async Task DeviceSelectsAutomaticallyDiscoveredDecoderWithoutRegistration()
    {
        using var f = new Fixture();
        f.Devices[0].DeviceProperties["TmcDecoder"] = nameof(AutomaticallyDiscoveredDecoder);
        using var response = await f.Report(1);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        using var result = JsonDocument.Parse(body);
        Assert.Equal(8, result.RootElement.GetProperty("charts")[0].GetProperty("totalVolume").GetInt32());
        Assert.Empty(f.Camera.Paths);
    }

    [Theory]
    [InlineData("VisionCameraAPI", 7)]
    [InlineData("AutomaticallyDiscoveredDecoder", 8)]
    public async Task OneDeviceCanSelectEitherConfiguredDecoder(string decoder, int total)
    {
        using var f = new Fixture();
        f.Devices[0].DeviceProperties["TmcDecoder"] = " VisionCameraAPI, AutomaticallyDiscoveredDecoder, VisionCameraAPI, ";
        using var response = await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData", new {
            source="devices", decoder, deviceIds=new[]{1}, locationIdentifier="1",
            start="2026-09-25T10:00:00", end="2026-09-25T11:00:00", binSize=15 });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(total, result.RootElement.GetProperty("charts").EnumerateArray().Sum(c => c.GetProperty("totalVolume").GetInt32()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("UnconfiguredDecoder")]
    public async Task MultipleDecodersRejectAmbiguousOrUnconfiguredSelection(string decoder)
    {
        using var f = new Fixture();
        f.Devices[0].DeviceProperties["TmcDecoder"] = "VisionCameraAPI,AutomaticallyDiscoveredDecoder";
        using var response = await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData", new {
            source="devices", decoder, deviceIds=new[]{1}, locationIdentifier="1",
            start="2026-09-25T10:00:00", end="2026-09-25T11:00:00", binSize=15 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(f.Camera.Paths);
    }

    [Fact] public async Task SelectedDecoderMustBeConfiguredOnEveryCamera()
    {
        using var f = new Fixture(2);
        f.Devices[0].DeviceProperties["TmcDecoder"] = "VisionCameraAPI,AutomaticallyDiscoveredDecoder";
        using var response = await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData", new {
            source="devices", decoder="AutomaticallyDiscoveredDecoder", deviceIds=new[]{1,2}, locationIdentifier="1",
            start="2026-09-25T10:00:00", end="2026-09-25T11:00:00", binSize=15 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(f.Camera.Paths);
    }

    [Fact] public async Task NewSourceOnlyNeedsRegistrationToUseExistingReportAndMetrics()
    {
        using var f=new Fixture(configure:s=>s.AddTmcCountSource<FutureCountSource>("future"));
        using var response=await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData",new {
            source="future",locationIdentifier="1",start="2026-09-25T10:00:00",end="2026-09-25T11:00:00",binSize=15 });
        var body=await response.Content.ReadAsStringAsync(); Assert.True(response.IsSuccessStatusCode,body);
        using var result=JsonDocument.Parse(body);
        Assert.Equal("Future source",result.RootElement.GetProperty("source").GetString());
        Assert.Equal(8,result.RootElement.GetProperty("charts")[0].GetProperty("totalVolume").GetInt32());
        Assert.Equal(0.25,result.RootElement.GetProperty("peakHourFactor").GetDouble());
        Assert.Empty(f.Camera.Paths);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task IndianaAndCameraSourcesUseSameBuilderAndMetrics(bool combine)
    {
        var movements=new[]{MovementTypes.T,MovementTypes.L,MovementTypes.R};
        var volumes=new[]{4,2,1};
        var rows=volumes.SelectMany((v,i)=>Enumerable.Range(0,v).Select(n=>new IndianaEvent {
            LocationIdentifier="1",Timestamp=new DateTime(2026,9,25,10,15,5+n),EventCode=82,EventParam=(short)(i+1) })).ToList();
        using var f=new Fixture(indiana:rows); f.Camera.ZoneName="NB-18";
        var location=f.Devices[0].Location;
        var approach=new Approach{Id=1,DirectionTypeId=DirectionTypes.NB,Location=location};
        location.Approaches.Add(approach);
        for(var i=0;i<3;i++) approach.Detectors.Add(new Detector {
            Id=i+1,Approach=approach,DetectorChannel=i+1,LaneNumber=1,LaneType=LaneTypes.V,MovementType=movements[i],
            DetectionTypes=new List<DetectionType>{new(){MeasureTypes=new List<MeasureType>{new(){Id=5}}}} });
        var results=new List<JsonDocument>();
        try {
            foreach(var source in new[]{"atspm","devices"}) {
                using var response=await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData",new {
                    source,deviceIds=new[]{1},locationIdentifier="1",start="2026-09-25T10:00:00",end="2026-09-25T11:00:00",binSize=15,combineThruRight=combine });
                var body=await response.Content.ReadAsStringAsync(); Assert.True(response.IsSuccessStatusCode,body);
                results.Add(JsonDocument.Parse(body));
            }
            string[] ChartCounts(JsonDocument doc)=>doc.RootElement.GetProperty("charts").EnumerateArray()
                .Select(c=>$"{c.GetProperty("direction")}|{c.GetProperty("movementType")}|{c.GetProperty("totalVolume")}|{c.GetProperty("totalVolumes")}").OrderBy(x=>x).ToArray();
            Assert.Equal(ChartCounts(results[0]),ChartCounts(results[1]));
            Assert.Equal(results[0].RootElement.GetProperty("peakHour").ToString(),results[1].RootElement.GetProperty("peakHour").ToString());
            Assert.Equal(results[0].RootElement.GetProperty("peakHourFactor").ToString(),results[1].RootElement.GetProperty("peakHourFactor").ToString());
        } finally { foreach(var result in results) result.Dispose(); }
    }
    [Theory] [InlineData(false)] [InlineData(null)]
    public async Task DirectDeviceSourceNameCannotBypassFeatureGate(bool? enabled)
    {
        using var f=new Fixture(enabled:enabled);
        using var response=await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData",new {
            source="VisionCameraAPI",deviceIds=new[]{1},locationIdentifier="1",start="2026-09-25T10:00:00",end="2026-09-25T11:00:00",binSize=15 });
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode); Assert.Empty(f.Camera.Paths);
    }
    [Theory] [InlineData(1)] [InlineData(4)] [InlineData(8)]
    public async Task SingleAndMultipleCamerasProduceOneSourceWithoutControllerDetectors(int count) {
        using var f=new Fixture(count); using var response=await f.Report(Enumerable.Range(1,count).ToArray());
        var body=await response.Content.ReadAsStringAsync(); Assert.True(response.IsSuccessStatusCode,body);
        using var result=JsonDocument.Parse(body); var charts=result.RootElement.GetProperty("charts").EnumerateArray().ToList();
        Assert.Equal(count*7,charts.Sum(c=>c.GetProperty("totalVolume").GetInt32())); Assert.Equal(3,charts.Count);
        Assert.Contains($"({count} camera",result.RootElement.GetProperty("source").GetString());
        Assert.Equal(count,f.Camera.Calls); Assert.InRange(f.Camera.Maximum,1,4);
        Assert.All(charts,c=>Assert.Equal("Unknown",c.GetProperty("plans")[0].GetProperty("planDescription").GetString()));
    }
    [Fact] public async Task SamePhysicalCameraConfiguredTwiceIsReadOnce() {
        using var f=new Fixture(2); f.Devices[1].DeviceIdentifier="523648495-bins";
        using var response=await f.Report(1,2); Assert.Equal(HttpStatusCode.OK,response.StatusCode); Assert.Equal(1,f.Camera.Calls);
        Assert.Contains("Duplicate camera",await response.Content.ReadAsStringAsync());
    }
    [Fact] public async Task MixedLiveAndStoredSelectionIsRejectedBeforeContactingCamera() {
        using var f=new Fixture(2);
        f.Devices[1].DeviceProperties["TmcDecoder"]=nameof(VisionBinStatisticsStored);
        using var response=await f.Report(1,2);
        Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        Assert.Empty(f.Camera.Paths);
    }
    [Fact] public async Task DatabaseDecoderReadsSelectedCamerasWithoutHttp()
    {
        async IAsyncEnumerable<CompressedEventLogs<VisionCameraStatisticsEvent>> Blocks(int id)
        {
            yield return new() { Data = new List<VisionCameraStatisticsEvent> { new() {
                Timestamp=new DateTime(2026,9,25,17,15,0,DateTimeKind.Utc), ZoneName=$"NB-{id}", ZoneId=id,
                Volume=id*7, ThroughCount=id*4, LeftTurnCount=id*2, RightTurnCount=id } } };
            await Task.CompletedTask;
        }
        var statistics=new Mock<IEventLogRepository>(MockBehavior.Strict);
        foreach (var id in new[]{1,2})
            statistics.Setup(r=>r.GetData<VisionCameraStatisticsEvent>("1",It.IsAny<DateTime>(),It.IsAny<DateTime>(),id)).Returns(()=>Blocks(id));
        using var f=new Fixture(2,statistics:statistics.Object);
        foreach(var device in f.Devices) {
            device.DeviceProperties["TmcDecoder"]=nameof(VisionBinStatisticsStored);
            device.LoggingEnabled=true;
            device.DeviceConfiguration.Decoders=new[]{"JsonToVisionCameraStatisticEventDecoder"};
        }
        using var response=await f.Report(1,2);
        var body=await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,body);
        using var result=JsonDocument.Parse(body);
        Assert.Equal(21,result.RootElement.GetProperty("charts").EnumerateArray().Sum(c=>c.GetProperty("totalVolume").GetInt32()));
        Assert.Equal("Vision Bin Statistics Stored (2 cameras)",result.RootElement.GetProperty("source").GetString());
        Assert.Empty(f.Camera.Paths);
        statistics.VerifyAll();
    }
    [Theory]
    [InlineData("NB-18", "Northbound")]
    [InlineData("North Bound", "Northbound")]
    [InlineData("Loading Dock", "Loading Dock")]
    [InlineData("Northgate", "Northgate")]
    public async Task ZoneDirectionsReachReportWithoutMapping(string zone, string direction) {
        using var f=new Fixture(); f.Camera.ZoneName=zone;
        using var response=await f.Report(1); var body=await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        using var result=JsonDocument.Parse(body);
        var charts=result.RootElement.GetProperty("charts").EnumerateArray().ToList();
        Assert.Equal(7, charts.Sum(c=>c.GetProperty("totalVolume").GetInt32()));
        Assert.All(charts, c=>Assert.Contains(direction, c.ToString()));
    }
    [Theory] [InlineData("wrong-location")] [InlineData("inactive")] [InlineData("unknown-decoder")] [InlineData("missing-decoder")] [InlineData("wrong-hardware")]
    public async Task InvalidDeviceIsRejectedBeforeAnyHardwareCall(string invalid) {
        using var f=new Fixture(); var d=f.Devices[0];
        if(invalid=="wrong-location")d.LocationId=2;
        if(invalid=="inactive")d.DeviceStatus=DeviceStatus.Inactive;
        if(invalid=="unknown-decoder")d.DeviceProperties["TmcDecoder"]="Unknown";
        if(invalid=="missing-decoder")d.DeviceProperties.Clear();
        if(invalid=="wrong-hardware")d.DeviceType=DeviceTypes.SignalController;
        using var response=await f.Report(1); Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode); Assert.Empty(f.Camera.Paths);
    }
    [Theory] [InlineData(false)] [InlineData(null)]
    public async Task FeatureFlagOffOrMissingRejectsDevicesButDefaultStillWorks(bool? enabled) {
        using var f=new Fixture(enabled:enabled, indiana:new List<IndianaEvent> {
            new() { LocationIdentifier="1", Timestamp=new DateTime(2026,9,25,10,15,5), EventCode=82, EventParam=1 }
        });
        var location=f.Devices[0].Location;
        var approach=new Approach { Id=1, DirectionTypeId=DirectionTypes.NB, Location=location };
        location.Approaches.Add(approach);
        approach.Detectors.Add(new Detector {
            Id=1, Approach=approach, DetectorChannel=1, LaneNumber=1, LaneType=LaneTypes.V, MovementType=MovementTypes.T,
            DetectionTypes=new List<DetectionType> { new() { MeasureTypes=new List<MeasureType> { new() { Id=5 } } } }
        });
        using var rejected=await f.Report(1);
        Assert.Equal(HttpStatusCode.BadRequest,rejected.StatusCode);
        using var response=await f.Client.PostAsJsonAsync("/api/v1/TurningMovementCounts/getReportData",new{locationIdentifier="1",start="2026-09-25T10:00:00",end="2026-09-25T11:00:00",binSize=15});
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        using var result=JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1,result.RootElement.GetProperty("charts")[0].GetProperty("totalVolume").GetInt32());
        Assert.Equal("Indiana Events (ATSPM)",result.RootElement.GetProperty("source").GetString());
        Assert.Empty(f.Camera.Paths);
    }
    [Theory] [InlineData(null, false)] [InlineData(false, false)] [InlineData(true, true)]
    public async Task FeatureDiscoveryUsesConfiguredValueAndDefaultsToFalse(bool? enabled, bool expected)
    {
        using var f = new Fixture(enabled: enabled);
        Assert.Equal(expected, await f.Client.GetFromJsonAsync<bool>("/api/v1/TurningMovementCounts/deviceSourcesEnabled"));
        Assert.Empty(f.Camera.Paths);
    }
    [Fact] public async Task PartialFailureWarnsAndAllOfflineIs503() {
        using var f=new Fixture(2); f.Camera.OfflineIndex="2";
        using var response=await f.Report(1,2); Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        using var result=JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(result.RootElement.GetProperty("warnings").EnumerateArray(),w=>w.GetString().Contains("offline"));
        Assert.Equal("Vision Camera API (1 of 2 cameras)",result.RootElement.GetProperty("source").GetString());
        using var failed=await f.Report(2); Assert.Equal(HttpStatusCode.ServiceUnavailable,failed.StatusCode);
        Assert.StartsWith("None of the selected devices responded.\nCamera 2: no data for",await failed.Content.ReadAsStringAsync());
    }
    [Fact] public async Task Camera400IsThatCamerasWarning() {
        using var f=new Fixture(2); f.Camera.BadRequestIndex="2";
        using var response=await f.Report(1,2); Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        using var result=JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains(result.RootElement.GetProperty("warnings").EnumerateArray(),w=>w.GetString().Contains("rejected the request: invalid camera date"));
        Assert.Equal("Vision Camera API (1 of 2 cameras)",result.RootElement.GetProperty("source").GetString());
    }
}
