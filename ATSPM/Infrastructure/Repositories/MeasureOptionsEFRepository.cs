using ATSPM.Application.Repositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Measure options entity framework repository
    /// </summary>
    public class MeasureOptionsEFRepository : ATSPMRepositoryEFBase<MeasureOption>, IMeasureOptionsRepository
    {
        /// <inheritdoc/>
        public MeasureOptionsEFRepository(ConfigContext db, ILogger<MeasureOptionsEFRepository> log) : base(db, log) { }

        #region Overrides

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
