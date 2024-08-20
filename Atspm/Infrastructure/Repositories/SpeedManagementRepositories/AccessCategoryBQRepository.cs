using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.SpeedManagementRepositories
{
    public class AccessCategoryBQRepository : NameAndIdBaseBQRepository, IAccessCategoryRepository
    {
        public AccessCategoryBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<NameAndIdDto>> log) : base(client, datasetId, tableId, log)
        {

        }
    }
}
