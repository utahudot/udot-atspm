using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.SpeedManagementRepositories
{
    public class FunctionalTypeBQRepository : NameAndIdBaseBQRepository, IFunctionalTypeRepository
    {
        public FunctionalTypeBQRepository(BigQueryClient client, string datasetId, string tableId, ILogger<ATSPMRepositoryBQBase<NameAndIdDto>> log) : base(client, datasetId, tableId, log)
        {
        }
    }
}
