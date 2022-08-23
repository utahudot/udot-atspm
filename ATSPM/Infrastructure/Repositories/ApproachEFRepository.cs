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

namespace ATSPM.Infrasturcture.Repositories
{
    public class ApproachEFRepository : ATSPMRepositoryEFBase<Approach>, IApproachRepository
    {
        public ApproachEFRepository(DbContext db, ILogger<ApproachEFRepository> log) : base(db, log) { }

        [Obsolete("Use Add in the BaseClass")]
        public void AddOrUpdate(Approach approach)
        {
            throw new NotImplementedException();
        }

        public Approach FindAppoachByVersionIdPhaseOverlapAndDirection(int versionId, int phaseNumber, bool isOverlap, int directionTypeId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetList instead")]
        public IReadOnlyList<Approach> GetAllApproaches()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead")]
        public Approach GetApproachByApproachID(int approachID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetList instead")]
        public IReadOnlyList<Approach> GetApproachesByIds(List<int> excludedApproachIds)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass")]
        public void Remove(int approachID)
        {
            throw new NotImplementedException();
        }
    }
}
