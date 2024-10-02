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
        var segments = new Option<List<string>?>("--segments", "segments");

        AddOption(new Option<int>("--sourceId", "sourceId"));
        AddOption(new Option<DateTime>("--startDate", "start date (mm-dd-yyyy)"));
        AddOption(new Option<DateTime>("--endDate", "end date (mm-dd-yyyy)"));
        if (filePath != null)
        {
            AddOption(filePath);
        }
        if (segments != null)
        {
            AddOption(segments);
        }


        this.Handler = CommandHandler.Create<int, DateTime, DateTime, string, List<string>>(Execute);
    }

    private async Task Execute(int sourceId, DateTime startDate, DateTime endDate, string filePath, List<string> segments)
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
                await _clearguideFileUploader.FileUploaderAsync(filePath);
                break;
            default:
                throw new ArgumentException("Invalid sourceId.");
        }
    }

}
