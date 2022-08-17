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
    public class DirectionTypeEFRepository : ATSPMRepositoryEFBase<DetectorEventCountAggregation>, IDirectionTypeRepository
    {
        public DirectionTypeEFRepository(DbContext db, ILogger<DirectionTypeEFRepository> log) : base(db, log)
        {
            
        }

        public IReadOnlyCollection<DirectionType> GetAllDirections()
        {
            return _db.Set<DirectionType>().OrderBy(d => d.DisplayOrder).ToList();
        }

        public DirectionType GetByDescription(string directionDescription)
        {
            return _db.Set<DirectionType>().Where(d => d.Description == directionDescription).FirstOrDefault();
        }

        public DirectionType GetDirectionByID(int directionID)
        {
            return _db.Set<DirectionType>().Where(d => d.DirectionTypeId == directionID).FirstOrDefault();
        }

        public IReadOnlyCollection<DirectionType> GetDirectionsByIDs(List<int> includedDirections)
        {
            return _db.Set<DirectionType>().Where(d => includedDirections.Contains(d.DirectionTypeId)).ToList();
        }

    }
}
