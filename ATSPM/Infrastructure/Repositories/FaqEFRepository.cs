using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    public class FaqEFRepository : ATSPMRepositoryEFBase<Faq>, IFaqRepository
    {
        public FaqEFRepository(ConfigContext db, ILogger<FaqEFRepository> log) : base(db, log) { }

        #region Overrides

        public override IQueryable<Faq> GetList()
        {
            return base.GetList().OrderBy(o => o.DisplayOrder);
        }

        #endregion

        #region IFaqRepository

        #endregion
    }
}
