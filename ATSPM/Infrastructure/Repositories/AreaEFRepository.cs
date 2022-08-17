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
    public class AreaEFRepository : ATSPMRepositoryEFBase<Area>, IAreaRepository
    {

        public AreaEFRepository(DbContext db, ILogger<AreaEFRepository> log) : base(db, log)
        {

        }

        public void DeleteByID(int areaId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Area> GetAllAreas()
        {
            return _db.Set<Area>().OrderBy(a => a.AreaName).ToList();
        }

        public Area GetAreaByID(int areaId)
        {
            return _db.Set<Area>().Where(a => a.Id == areaId).FirstOrDefault();
        }

        public Area GetAreaByName(string AreaName)
        {
            return _db.Set<Area>().Where(a => a.AreaName == AreaName).FirstOrDefault();
        }

        public IReadOnlyCollection<Area> GetListOfAreasForSignal(string signalId)
        {
            throw new NotImplementedException();
        }
    }
}
