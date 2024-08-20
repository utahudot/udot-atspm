using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
{
    public class CityBQRepository : NameAndIdBaseBQRepository, ICityRepository
    {
        public CityBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<NameAndIdDto>> log) : base(client, datasetId, tableId, log)
        {
        }
    }
}
