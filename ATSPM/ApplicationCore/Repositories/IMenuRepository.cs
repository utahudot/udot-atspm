using ATSPM.Data.Models;
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
        IReadOnlyList<Menu> GetTopLevelMenuItems(string Application);

        [Obsolete("Use GetList instead")]
        IReadOnlyList<Menu> GetAll(string Application);
        
        [Obsolete("Use Lookup instead")]
        Menu GetMenuItembyID(int id);
        
        [Obsolete("Use Add in the BaseClass")]
        void Add(Menu menuItem);
       
        [Obsolete("Use Remove in the BaseClass")]
        void Remove(int id);
        
        [Obsolete("Use Update in the BaseClass")]
        void Update(Menu menuItem);
    }
}
