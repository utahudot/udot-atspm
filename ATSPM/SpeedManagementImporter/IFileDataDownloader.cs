namespace SpeedManagementImporter
{
    public interface IFileDataDownloader : IDataDownloader
    {
        public Task Download(string filePath);
    }
}
