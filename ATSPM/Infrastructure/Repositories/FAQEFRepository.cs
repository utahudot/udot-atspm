using ATSPM.Application.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public class FAQEFRepository : ATSPMRepositoryEFBase<Faq>, IFAQRepository
    {
        public FAQEFRepository(DbContext db, ILogger<FAQEFRepository> log) : base(db, log)
        {

        }

        public List<Faq> GetAll()
        {
            throw new NotImplementedException();
        }

        public Faq GetbyID(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
