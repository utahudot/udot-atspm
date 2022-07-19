using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories
{
    public interface IMenuRepository : IAsyncRepository<Menu>
    {
        [Obsolete("Use GetList instead")]
        IReadOnlyCollection<Menu> GetAll(string Application);
        [Obsolete("Use Lookup instead")]
        Menu GetMenuItembyID(int id);
        [Obsolete("Use Add in the BaseClass")]
        void Add(Menu menuItem);
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
        [Obsolete("Use Update in the BaseClass")]
        void Update(Menu menuItem);
        IReadOnlyCollection<Menu> GetTopLevelMenuItems(string Application);
    }
}
