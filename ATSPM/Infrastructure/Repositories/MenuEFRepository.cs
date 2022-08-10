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
    public class MenuEFRepository : ATSPMRepositoryEFBase<Menu>, IMenuRepository
    {
        public MenuEFRepository(DbContext db, ILogger<MenuEFRepository> log) : base(db, log)
        {

        }

        public IReadOnlyCollection<Menu> GetAll(string Application)
        {
            throw new NotImplementedException();
        }

        public Menu GetMenuItembyID(int id)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Menu> GetTopLevelMenuItems(string Application)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
