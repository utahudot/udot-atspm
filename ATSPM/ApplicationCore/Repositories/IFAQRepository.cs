using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IFAQRepository : IAsyncRepository<Faq>
    {
        [Obsolete("Use GetList instead")]
        List<Faq> GetAll();
        [Obsolete("Use Lookup instead")]
        Faq GetbyID(int id);
        [Obsolete("Use Add in the BaseClass")]
        void Add(Faq item);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
        [Obsolete("Use Update in the BaseClass")]
        void Update(Faq item);
    }
}
}
