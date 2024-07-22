using SpeedManagementDataDownloader.Business.Common.DataDownloader;

namespace SpeedManagementDataDownloader.Business.Common.ImporterFactory
{
    public interface IImporterFactory
    {
        IDataDownloader createDownloader(int sourceId);
    }
}
