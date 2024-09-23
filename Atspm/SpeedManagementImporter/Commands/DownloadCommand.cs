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

    public DownloadCommand(
        PemsDownloaderService pemsDownloader,
        AtspmDownloaderService atspmDownloader,
        ClearguideFileDownloaderService clearguideDownloader)
        : base("download", "Download data from different sources")
    {
        _pemsDownloader = pemsDownloader;
        _atspmDownloader = atspmDownloader;
        _clearguideDownloader = clearguideDownloader;

        AddOption(new Option<int>("--sourceId", "sourceId"));
        AddOption(new Option<DateTime>("--startDate", "start date (mm-dd-yyyy)"));
        AddOption(new Option<DateTime>("--endDate", "end date (mm-dd-yyyy)"));

        this.Handler = CommandHandler.Create<int, DateTime, DateTime>(Execute);
    }

    private async Task Execute(int sourceId, DateTime startDate, DateTime endDate)
    {
        switch (sourceId)
        {
            case 1:
                await _atspmDownloader.Download(startDate, endDate);
                break;
            case 2:
                await _pemsDownloader.Download(startDate, endDate);
                break;
            case 3:
                await _clearguideDownloader.Download(startDate, endDate);
                break;
            default:
                throw new ArgumentException("Invalid sourceId.");
        }
    }

}
