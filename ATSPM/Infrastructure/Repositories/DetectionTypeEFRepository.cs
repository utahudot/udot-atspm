using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATSPM.Infrasturcture.Repositories
{
    public class DetectionTypeEFRepository : ATSPMRepositoryEFBase<DetectionType>, IDetectionTypeRepository
    {
        public DetectionTypeEFRepository(DbContext db, ILogger<DetectionTypeEFRepository> log) : base(db, log) { }

        public List<DetectionType> GetAllDetectionTypes()
        {
            throw new NotImplementedException();
        }

        public List<DetectionType> GetAllDetectionTypesNoBasic()
        {
            throw new NotImplementedException();
        }

        public DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Exists in base class")]
        public void Update(DetectionType detectionType)
        {
            //var g = (from r in _db.DetectionTypes
            //         where r.DetectionTypeID == detectionType.DetectionTypeID
            //         select r).FirstOrDefault();
            //if (g != null)
            //{
            //    _db.Entry(g).CurrentValues.SetValues(detectionType);
            //    _db.SaveChanges();
            //}
            //else
            //{
            //    _db.DetectionTypes.Add(detectionType);
            //    _db.SaveChanges();
            //}

            throw new NotImplementedException();
        }
    }
}
