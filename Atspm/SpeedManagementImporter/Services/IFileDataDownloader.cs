namespace SpeedManagementImporter.Services
{
    public interface IFileDataDownloader : IDataDownloader
    {
        public Task Download(string filePath);
    }
}
