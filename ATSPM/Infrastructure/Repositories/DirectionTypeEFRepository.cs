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
            throw new NotImplementedException();
        }

        public DirectionType GetByDescription(string directionDescription)
        {
            throw new NotImplementedException();
        }

        public DirectionType GetDirectionByID(int directionID)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<DirectionType> GetDirectionsByIDs(List<int> includedDirections)
        {
            throw new NotImplementedException();
        }

    }
}
