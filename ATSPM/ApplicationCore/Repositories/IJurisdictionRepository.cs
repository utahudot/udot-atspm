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
    /// Jurisdiction Repository
    /// </summary>
    public interface IJurisdictionRepository : IAsyncRepository<Jurisdiction>
    {
    }
}
