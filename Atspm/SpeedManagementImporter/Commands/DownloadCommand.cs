using SpeedManagementImporter.Services.Atspm;
using SpeedManagementImporter.Services.Clearguide;
using SpeedManagementImporter.Services.Pems;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

public class DownloadCommand : Command
{
    private readonly PemsDownloaderService _pemsDownloader;
    private readonly AtspmDownloaderService _atspmDownloader;
    private readonly ClearguideFileDownloaderService _clearguideDownloader;
    private readonly ClearguideFileUploader _clearguideFileUploader;

    public DownloadCommand(
        PemsDownloaderService pemsDownloader,
        AtspmDownloaderService atspmDownloader,
        ClearguideFileDownloaderService clearguideDownloader,
        ClearguideFileUploader clearguideFileUploader)
        : base("download", "Download data from different sources")
    {
        _pemsDownloader = pemsDownloader;
        _atspmDownloader = atspmDownloader;
        _clearguideDownloader = clearguideDownloader;
        _clearguideFileUploader = clearguideFileUploader;
        var filePath = new Option<string?>("--filePath", "filePath");
        var segments = new Option<List<string>?>(
            "--segments",
            description: "segments",
            parseArgument: result => result.Tokens.Select(t => t.Value.Split(',')).SelectMany(x => x).ToList());

        AddOption(new Option<int>("--sourceId", "sourceId"));
        AddOption(new Option<DateTime>("--startDate", "start date (mm-dd-yyyy)"));
        AddOption(new Option<DateTime>("--endDate", "end date (mm-dd-yyyy)"));
        AddOption(new Option<string>("--operation", "operation"));
        if (filePath != null)
        {
            AddOption(filePath);
        }
        if (segments != null)
        {
            AddOption(segments);
        }


        this.Handler = CommandHandler.Create<int, DateTime, DateTime, string, string, List<string>>(Execute);
    }

    private async Task Execute(int sourceId, DateTime startDate, DateTime endDate, string operation, string filePath, List<string> segments)
    {
        if (operation.ToLower() == "download")
        {
            switch (sourceId)
            {
                case 1:
                    await _atspmDownloader.Download(startDate, endDate, segments);
                    break;
                case 2:
                    await _pemsDownloader.Download(startDate, endDate, segments);
                    break;
                case 3:
                    await _clearguideDownloader.Download(startDate, endDate, segments);
                    break;
                case 4:
                    await _clearguideFileUploader.FileUploaderAsync(filePath, segments);
                    break;
                default:
                    throw new ArgumentException("Invalid sourceId.");
            }
        }
        if (operation.ToLower() == "delete" && segments.Count > 0)
        {
            switch (sourceId)
            {
                case 2:
                    await _pemsDownloader.DeleteSegmentData(segments);
                    break;
                case 3:
                    await _clearguideDownloader.DeleteSegmentData(segments);
                    break;
                default:
                    throw new ArgumentException("Invalid sourceId.");
            }
        }
    }

}
