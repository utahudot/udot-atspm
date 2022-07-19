using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ATSPM.Application.Repositories
{
    public interface IJurisdictionRepository : IAsyncRepository<Jurisdiction>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<Jurisdiction> GetAllJurisdictions();
        [Obsolete("Use Lookup instead")]
        Jurisdiction GetJurisdictionByID(int jurisdictionId);
        [Obsolete("Use Lookup instead")]
        Jurisdiction GetJurisdictionByName(string jurisdictionName);
        [Obsolete("Use Delete in the BaseClass")]
        void DeleteByID(int jurisdictionId);
        [Obsolete("Use Update in the BaseClass")]
        void Update(Jurisdiction newJurisdiction);
        [Obsolete("Use Add in the BaseClass")]
        void Add(Jurisdiction newJurisdiction);
    }
}
