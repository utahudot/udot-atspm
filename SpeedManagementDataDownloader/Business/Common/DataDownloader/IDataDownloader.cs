using System;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Business.Common.DataDownloader
{
    public interface IDataDownloader
    {
        Task Download(DateTime startDate, DateTime endDate);
    }
}
