using Utah.Udot.Atspm.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.TurningMovementCounts.Decoders.EconoliteVision;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Xunit;

namespace InfrastructureTests.TurningMovementCounts;
public sealed class CameraHandler : HttpMessageHandler
{
    public readonly List<Uri> Requests = new();
    public readonly List<string> Authorizations = new();
    public Func<Uri, HttpResponseMessage> Response { get; set; } = uri => new(HttpStatusCode.OK) { Content = new StringContent(
        uri.AbsolutePath.EndsWith("device-info") ? "{\"name\":\"O420 02 NB 05 LT\"}" :
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TurningMovementCounts/Fixtures", uri.Query.Contains("interval=5") ? "vision-5-minute.json" : "vision-15-minute.json"))) };
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
    { lock(Requests) { Requests.Add(request.RequestUri); Authorizations.Add(request.Headers.Authorization?.ToString()); } return Task.FromResult(Response(request.RequestUri)); }
}
public class VisionCameraAPITests : ApplicationTests.Business.TurningMovementCounts.TurningMovementCountDecoderContractTests<VisionCameraAPI>
{
    public static TmcDecodeRequest Request(int bin = 15) {
        var location = new Location { Id = 1, LocationIdentifier = "1", Latitude = 33.35, Longitude = -111.79 };
        var device = new Device { Id = 1, LocationId = 1, Location = location, DeviceIdentifier = "1", Ipaddress = "127.0.0.1",
            DeviceStatus = DeviceStatus.Active, DeviceType = DeviceTypes.FIRCamera, LoggingEnabled = true,
            DeviceConfiguration = new DeviceConfiguration { Port = 8080, Path = "/api/v1/cameras", Product = new Product { Manufacturer = "Econolite", Model = "Vision Camera" }, Decoders = new[] { "JsonToVisionCameraStatisticEventDecoder" } },
            DeviceProperties = new() { ["TmcDecoder"] = nameof(VisionCameraAPI) } };
        return new(location, device, device.DeviceProperties, new TurningMovementCountsOptions { Source = "devices", DeviceIds = new[]{1}, LocationIdentifier = "1", Start = new(2026,9,25,10,0,0), End = new(2026,9,25,11,0,0), BinSize = bin });
    }
    public static VisionCameraAPI Decoder(CameraHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("TmcCamera")).Returns(new HttpClient(handler));
        return new(factory.Object, new TmcCameraConcurrency());
    }
    private static VisionBinStatisticsStored StoredDecoder(IEventLogRepository repository = null)
    {
        var services = new ServiceCollection();
        if (repository != null) services.AddSingleton(repository);
        return new(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
    }
    protected override (VisionCameraAPI, TmcDecodeRequest) Create() => (Decoder(new()), Request());
    [Fact]
    public async Task RejectedLoginStopsRemainingDaysForThisCamera()
    {
        var request = Request();
        request.Options.End = request.Options.Start.AddDays(31);
        var handler = new CameraHandler { Response = _ => new(HttpStatusCode.Unauthorized) { Content = new StringContent("") } };
        var result = await Decoder(handler).DecodeAsync(request, default);
        Assert.Single(handler.Requests);
        Assert.Single(result.Warnings.Where(w => w.Contains("no data for") && w.Contains("login rejected")));
        Assert.Contains(result.Warnings, w => w.Contains("skipped"));
    }
    [Theory]
    [InlineData("/custom/v2/cameras")]
    [InlineData("/custom/v2/cameras/")]
    [InlineData("/custom/v2/cameras/99/detections")]
    [InlineData("/custom/v2/cameras/99/bin-statistics")]
    [InlineData("/custom/v2/cameras/[Device:DeviceIdentifier]/detections")]
    public async Task ConfiguredPathIsUsedForDiscoveryAndStatistics(string path)
    {
        var request = Request();
        request.Device.DeviceConfiguration.Path = path;
        var handler = new CameraHandler();
        var decoder = Decoder(handler);
        await decoder.IdentityAsync(request.Device, default);
        var result = await decoder.DecodeAsync(request, default);
        Assert.Equal(4, result.Counts.Sum(c => c.Volume));
        Assert.Equal("/custom/v2/cameras", handler.Requests[0].AbsolutePath);
        Assert.Equal("/custom/v2/cameras/1/bin-statistics", handler.Requests[1].AbsolutePath);
        Assert.Contains("start-time=2026-09-25T17:00:00Z", handler.Requests[1].Query);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://other-host/cameras")]
    [InlineData("//other-host/cameras")]
    [InlineData("/cameras?invalid=query")]
    public async Task InvalidConfiguredPathFailsBeforeHttp(string path)
    {
        var request = Request();
        request.Device.DeviceConfiguration.Path = path;
        var handler = new CameraHandler();
        var error = await Assert.ThrowsAsync<ReportException>(() => Decoder(handler).DecodeAsync(request, default));
        Assert.Equal(400, error.StatusCode);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task DiscoveryCacheSeparatesConfiguredPathsOnSameManager()
    {
        var handler = new CameraHandler();
        var decoder = Decoder(handler);
        var request = Request();
        await decoder.IdentityAsync(request.Device, default);
        request.Device.DeviceConfiguration.Path = "/other/cameras";
        await decoder.IdentityAsync(request.Device, default);
        Assert.Equal(new[] { "/api/v1/cameras", "/other/cameras" }, handler.Requests.Select(u => u.AbsolutePath));
    }

    [Theory]
    [InlineData("NB-18", DirectionTypes.NB)]
    [InlineData("eb2", DirectionTypes.EB)]
    [InlineData("WB Lane 3", DirectionTypes.WB)]
    [InlineData("sb-36", DirectionTypes.SB)]
    [InlineData("North", DirectionTypes.NB)]
    [InlineData("northbound-18", DirectionTypes.NB)]
    [InlineData("North Bound Lane 2", DirectionTypes.NB)]
    [InlineData("eastbound", DirectionTypes.EB)]
    [InlineData("West", DirectionTypes.WB)]
    [InlineData("SOUTH BOUND 2", DirectionTypes.SB)]
    [InlineData("North-East Bound-1", DirectionTypes.NE)]
    [InlineData("Camera 2 WB-3", DirectionTypes.WB)]
    public void DirectionNamesAreRecognizedWithoutConfiguration(string name, DirectionTypes direction) {
        var mapped = VisionZoneMapper.Map(Request(), new[]{Row(name,4,4,0,0)},15,TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix"),false);
        Assert.Equal(4,mapped.Counts.Sum(c=>c.Volume));
        Assert.All(mapped.Counts,c=>{Assert.Equal(direction,c.Direction);Assert.Null(c.DirectionLabel);});
    }
    [Theory]
    [InlineData("Loading Dock")]
    [InlineData("Northgate-1")]
    [InlineData("Westwood")]
    [InlineData("L51-2")]
    [InlineData("NEON")]
    [InlineData("NB / SB")]
    public void UnmatchedNamesBecomeTheirOwnDirections(string name) {
        var mapped = VisionZoneMapper.Map(Request(), new[]{Row(name,4,4,0,0)},15,TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix"),false);
        Assert.Equal(4,mapped.Counts.Sum(c=>c.Volume));
        Assert.All(mapped.Counts,c=>{Assert.Equal(DirectionTypes.NA,c.Direction);Assert.Equal(name,c.DirectionLabel);});
    }
    [Fact] public async Task LiveUsesUtcIntervalAndLocalBinStartLabels() {
        var handler = new CameraHandler(); var result = await Decoder(handler).DecodeAsync(Request(5), default);
        Assert.Equal(4, result.Counts.Sum(c => c.Volume));
        Assert.Contains(result.Counts, c => c.BinStart == new DateTime(2026,9,25,10,20,0));
        var uri = handler.Requests.Single(u => u.AbsolutePath.EndsWith("bin-statistics"));
        Assert.Contains("start-time=2026-09-25T17:00:00Z", uri.Query); Assert.Contains("interval=5",uri.Query);
    }
    [Theory] [InlineData("Other", "Vision")] [InlineData("Econolite", "Other")]
    public void RejectsWrongHardware(string make,string model) {
        var request=Request(); request.Device.DeviceConfiguration.Product.Manufacturer=make; request.Device.DeviceConfiguration.Product.Model=model;
        Assert.False(Decoder(new()).CanDecode(request.Device));
    }
    [Theory] [InlineData(9,16)] [InlineData(1,17)]
    public async Task UtahQueryUsesSeasonalUtcOffset(int month, int utcHour) {
        var request=Request(); request.Location.Latitude=39.32; request.Location.Longitude=-111.09;
        request.Options.Start=new DateTime(2026,month,25,10,0,0); request.Options.End=request.Options.Start.AddHours(1);
        var handler=new CameraHandler(); await Decoder(handler).DecodeAsync(request,default);
        Assert.Contains($"start-time=2026-{month:00}-25T{utcHour:00}:00:00Z", handler.Requests.Single().Query);
    }
    [Fact] public async Task SplitsDaysAndRetriesInvalidJsonAsTwoAlignedHalfDays() {
        var handler = new CameraHandler(); int bins=0;
        handler.Response=uri=>new(HttpStatusCode.OK){Content=new StringContent(uri.AbsolutePath.EndsWith("device-info")?"{}":++bins==1?"{broken":"{\"statistics\":[]}")};
        var request=Request(); request.Options.End=request.Options.Start.AddDays(2);
        await Decoder(handler).DecodeAsync(request,default); Assert.Equal(4,bins);
        Assert.Contains(handler.Requests,u=>u.Query.Contains("end-time=2026-09-26T05:00:00Z"));
    }
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Camera 1: no data for 2026-09-25 (rejected the request: bad date).")]
    [InlineData(HttpStatusCode.Unauthorized, "Camera 1: no data for 2026-09-25 (login rejected; check the username and password on the device configuration).")]
    public async Task RejectedRequestIsThatCamerasWarning(HttpStatusCode status, string warning) {
        var handler=new CameraHandler { Response=_=>new(status){Content=new StringContent("bad date")} };
        var result=await Decoder(handler).DecodeAsync(Request(),default);
        Assert.False(result.Responded); Assert.Contains(warning,result.Warnings);
    }
    [Fact] public async Task UnreachableCameraIsWarningAndRemainingDaysAreSkipped() {
        var handler=new CameraHandler { Response=_=>throw new HttpRequestException(HttpRequestError.ConnectionError,"refused") };
        var request=Request(); request.Options.End=request.Options.Start.AddDays(3);
        var result=await Decoder(handler).DecodeAsync(request,default);
        Assert.False(result.Responded); Assert.Single(handler.Requests);
        Assert.Contains("Camera 1: unreachable; skipped 2026-09-26 to 2026-09-27.",result.Warnings);
    }
    [Fact] public async Task OtherFailuresStillTryEveryDay() {
        var handler=new CameraHandler { Response=_=>new(HttpStatusCode.InternalServerError) };
        var request=Request(); request.Options.End=request.Options.Start.AddDays(3);
        await Decoder(handler).DecodeAsync(request,default); Assert.Equal(3,handler.Requests.Count);
    }
    [Fact] public async Task ConfiguredLoginIsSentAsBasicCredentials() {
        var handler=new CameraHandler(); var request=Request();
        await Decoder(handler).DecodeAsync(request,default); Assert.Null(handler.Authorizations.Single());
        request.Device.DeviceConfiguration.UserName="admin"; request.Device.DeviceConfiguration.Password="secret";
        await Decoder(handler).DecodeAsync(request,default);
        Assert.Equal("Basic "+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("admin:secret")),handler.Authorizations.Last());
    }
    [Fact] public async Task StoredRejectsSubFifteenMinuteBinsBeforeReading() {
        var request=Request(5); request.Device.DeviceProperties["TmcDecoder"]=nameof(VisionBinStatisticsStored);
        Assert.Equal(400,(await Assert.ThrowsAsync<ReportException>(()=>StoredDecoder().DecodeAsync(request,default))).StatusCode);
    }
    [Fact] public void LayerAndMovementMappingNeverUsesZoneTypeOrVolumeAsMovement() {
        var request=Request(); request.Device.DeviceProperties.Remove("TmcZoneMap");
        var rows=new[]{Row("N21-1",100,2,3,4),Row("A221-10",100,10,0,0),Row("RR21-3",100,10,0,0),Row("P2-64",0,0,0,0)};
        rows[3].LeftToRightCount=2; rows[3].RightToLeftCount=3;
        var mapped=VisionZoneMapper.Map(request,rows,15,TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix"),false);
        Assert.Equal(14,mapped.Counts.Sum(c=>c.Volume)); Assert.Contains(mapped.Counts,c=>c.Movement==MovementTypes.L&&c.Volume==3);
        Assert.Contains(mapped.Counts,c=>c.LaneType==LaneTypes.Ped&&c.Volume==5); Assert.Equal(2,mapped.Warnings.Count);
    }
    [Fact] public void AdvanceUsesOnlyClosestSetbackAndUnknownPhaseKeepsLabel() {
        var request=Request(); request.Device.DeviceProperties["TmcLayer"]="Advance";
        var mapped=VisionZoneMapper.Map(request,new[]{Row("A221-1",4,4,0,0),Row("A231-2",7,7,0,0)},15,TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix"),false);
        Assert.Equal(4,mapped.Counts.Sum(c=>c.Volume)); Assert.All(mapped.Counts,c=>Assert.Equal("A221-1",c.DirectionLabel));
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClosestSetbackIsChosenBeforeTimeFiltering(bool stored) {
        var request = Request();
        request.Device.DeviceProperties["TmcLayer"] = "Advance";
        var near = Row("A221-1", 4, 4, 0, 0);
        var far = Row("A231-2", 7, 7, 0, 0);
        near.Timestamp = near.Timestamp.AddHours(-1);
        var zone = TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix");
        if (stored) {
            near.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(near.Timestamp, zone);
            far.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(far.Timestamp, zone);
        }
        var mapped = VisionZoneMapper.Map(request, new[] { near, far }, 15, zone, stored);
        Assert.Empty(mapped.Counts);
        Assert.Contains(mapped.Warnings, w => w.Contains("closest setback"));
    }

    [Fact] public void LegacyManualMappingIsIgnoredAndRepeatedBinKeepsLastRow() {
        var request=Request(); request.Device.DeviceProperties["TmcZoneMap"]="{\"N21-1\":{\"approach\":\"Southbound\",\"lane\":2}}";
        var mapped=VisionZoneMapper.Map(request,new[]{Row("N21-1",1,1,0,0),Row("N21-1",4,4,0,0)},15,TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix"),false);
        Assert.Equal(4,mapped.Counts.Sum(c=>c.Volume)); Assert.All(mapped.Counts,c=>Assert.Equal(DirectionTypes.NB,c.Direction));
    }
    [Fact] public void LocalStoredRowsAndUtcLiveRowsMapIdentically() {
        var request=Request(); var row=Row("NB-18",4,4,0,0); var zone=TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix");
        var live=VisionZoneMapper.Map(request,new[]{row},15,zone,false);
        row.Timestamp=TimeZoneInfo.ConvertTimeFromUtc(row.Timestamp,zone);
        var stored=VisionZoneMapper.Map(request,new[]{row},15,zone,true);
        Assert.Equal(live.Counts,stored.Counts);
    }
    [Theory] [InlineData(true)] [InlineData(false)] public async Task StoredReadsOnlySelectedDeviceAndMatchesLiveCountsWithoutHttp(bool utc) {
        var request = Request();
        var live = await Decoder(new()).DecodeAsync(request, default);
        using var stream = new MemoryStream(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TurningMovementCounts/Fixtures/vision-15-minute.json")));
        var rows = new Utah.Udot.ATSPM.Infrastructure.Services.EventLogDecoders.JsonToVisionCameraStatisticEventDecoder()
            .Decode(request.Device, stream, default).ToList();
        if (!utc) foreach (var row in rows) row.Timestamp = TimeZoneInfo.ConvertTimeFromUtc(row.Timestamp, TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix"));
        async IAsyncEnumerable<CompressedEventLogs<VisionCameraStatisticsEvent>> Blocks() {
            yield return new() { Data = rows }; await Task.CompletedTask;
        }
        var repository = new Mock<IEventLogRepository>(MockBehavior.Strict);
        repository.Setup(r => r.GetData<VisionCameraStatisticsEvent>("1", request.Options.Start, request.Options.End, 1)).Returns(Blocks());
        repository.Setup(r => r.GetData<VisionCameraStatisticsEvent>("1", new DateTime(2026,9,25,17,0,0,DateTimeKind.Utc), new DateTime(2026,9,25,18,0,0,DateTimeKind.Utc), 1)).Returns(Blocks());
        request.Device.DeviceProperties["TmcDecoder"] = nameof(VisionBinStatisticsStored);
        request.Device.DeviceProperties["TmcCameraName"] = "O420 02 NB 05 LT";
        var handler = new CameraHandler();
        var stored = await StoredDecoder(repository.Object).DecodeAsync(request, default);
        Assert.Equal(live.Counts, stored.Counts);
        Assert.Empty(handler.Requests);
        repository.VerifyAll();
    }
    private static VisionCameraStatisticsEvent Row(string name,int volume,int through,int left,int right)=>new(){ZoneName=name,ZoneId=1,
        Timestamp=new DateTime(2026,9,25,17,15,0,DateTimeKind.Utc),Volume=volume,ThroughCount=through,LeftTurnCount=left,RightTurnCount=right};
}
