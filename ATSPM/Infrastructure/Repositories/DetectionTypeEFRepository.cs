using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using ATSPM.Domain.Specifications;

namespace ATSPM.Infrastructure.Repositories
{
    public class DetectionTypeEFRepository : ATSPMRepositoryEFBase<DetectionType>, IDetectionTypeRepository
    {
        public DetectionTypeEFRepository(DbContext db, ILogger<DetectionTypeEFRepository> log) : base(db, log) { }

        public IReadOnlyList<DetectionType> GetAllDetectionTypes()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<DetectionType> GetAllDetectionTypesNoBasic()
        {
            throw new NotImplementedException();
        }

        public DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID)
        {
            throw new NotImplementedException();
        }
    }
}
