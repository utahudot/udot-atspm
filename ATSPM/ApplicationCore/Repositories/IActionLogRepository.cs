using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    /// <summary>
    /// Action Log Repository
    /// </summary>
    public interface IActionLogRepository : IAsyncRepository<ActionLog>
    {
        #region ExtensionMethods

        //IReadOnlyList<ActionLog> GetAllByDate(DateTime startDate, DateTime endDate);

        #endregion
    }
}
