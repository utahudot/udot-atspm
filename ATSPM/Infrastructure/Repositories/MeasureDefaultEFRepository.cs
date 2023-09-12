using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.ValueObjects;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ATSPM.Application.Specifications;
using ATSPM.Domain.Services;
using ATSPM.Data;

namespace ATSPM.Infrastructure.Repositories
{
    public class MeasureDefaultEFRepository : ATSPMRepositoryEFBase<MeasuresDefault>, IMeasuresDefaultsRepository
    {
        public MeasureDefaultEFRepository(ConfigContext db, ILogger<MeasureDefaultEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IMeasuresDefaultsRepository

        public IReadOnlyList<MeasuresDefault> GetAll()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetAllAsDictionary()
        {
            throw new NotImplementedException();
        }

        public IQueryable<string> GetListOfMeasures()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<MeasuresDefault> GetMeasureDefaults(string chart)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
