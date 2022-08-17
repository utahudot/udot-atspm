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
            return _db.Set<Menu>().ToList();
        }

        public Menu GetMenuItembyID(int id)
        {
            return _db.Set<Menu>().Find(id);
        }

        public IReadOnlyCollection<Menu> GetTopLevelMenuItems(string Application)
        {
            return _db.Set<Menu>().Where(m => m.Application == Application && m.ParentId == 0).OrderBy(x => x.DisplayOrder).ToList();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }
    }
}
