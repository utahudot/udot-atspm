using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IMeasureOptionsRepository"/>
    public class MeasureOptionsEFRepository : ATSPMRepositoryEFBase<MeasureOption>, IMeasureOptionsRepository
    {
        /// <inheritdoc/>
        public MeasureOptionsEFRepository(ConfigContext db, ILogger<MeasureOptionsEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<MeasureOption> GetList()
        {
            return base.GetList().Include(i => i.MeasureType);
        }

        #endregion

        #region IMeasureOptionsRepository

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MeasureOption> GetAll();

        //[Obsolete("Use GetList instead")]
        //IQueryable<string> GetListOfMeasures();

        //[Obsolete("Use Lookup instead")]
        //IReadOnlyList<MeasureOption> GetMeasureDefaults(string chart);

        //[Obsolete("Use GetList and ToDictionary instead")]
        //Dictionary<string, string> GetAllAsDictionary();

        //[Obsolete("Use GetList and ToDictionary instead")]
        //Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(MeasureOption option);

        #endregion
    }
}
