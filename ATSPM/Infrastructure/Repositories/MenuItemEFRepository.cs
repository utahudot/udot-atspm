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
using ATSPM.Data;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Menu item entity framework repository
    /// </summary>
    public class MenuItemEFRepository : ATSPMRepositoryEFBase<MenuItem>, IMenuItemReposiotry
    {
        /// <inheritdoc/>
        public MenuItemEFRepository(ConfigContext db, ILogger<MenuItemEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<MenuItem> GetList()
        {
            return base.GetList()
                .Include(i => i.Parent)
                .Include(i => i.Children)
                .OrderBy(o => o.DisplayOrder);
        }

        #endregion

        #region IMenuItemReposiotry

        #endregion
    }
}
