
namespace SpeedManagementImporter
{
    public interface IImporterFactory
    {
        public IDataDownloader createDownloader(int sourceId);
    }
}
