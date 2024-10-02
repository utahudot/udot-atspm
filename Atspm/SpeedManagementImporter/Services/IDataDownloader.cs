namespace SpeedManagementImporter
{
    public interface IDataDownloader
    {
        public Task Download(DateTime startDate, DateTime endDate, List<string>? providedSegments);
    }
}
