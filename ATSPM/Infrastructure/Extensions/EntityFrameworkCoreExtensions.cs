using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Infrastructure.Extensions
{
    public static class EntityFrameworkCoreExtensions
    {
        public static string CreateKeyValueName(this DbContext db, ATSPMModelBase item)
        {
            return item.GetType().Name + "_" + string.Join("_", db.Model.FindEntityType(item.GetType()).FindPrimaryKey().Properties.Select(p => string.Format(p.FindAnnotation("KeyNameFormat") != null ? "{0:" + p.FindAnnotation("KeyNameFormat")?.Value.ToString() + "}" : "{0}", p.PropertyInfo.GetValue(item, null))));
        }
    }
}
