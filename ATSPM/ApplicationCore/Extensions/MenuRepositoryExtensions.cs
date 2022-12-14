using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class MenuRepositoryExtensions
    {
        public static IReadOnlyList<Menu> GetTopLevelMenuItems(this IMenuRepository repo, string application)
        {
            return repo.GetList(new MenuTopLevelSpecification(application));
        }
    }
}
