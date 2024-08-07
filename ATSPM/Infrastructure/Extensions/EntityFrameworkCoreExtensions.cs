﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Extensions/EntityFrameworkCoreExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore;

namespace Utah.Udot.Atspm.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions for entity framework
    /// </summary>
    public static class EntityFrameworkCoreExtensions
    {
        /// <summary>
        /// Generates the key value name of an entity type from <paramref name="db"/>
        /// </summary>
        /// <param name="db"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string CreateKeyValueName(this DbContext db, object item)
        {
            return item.GetType().Name + "_" + string.Join("_", db.Model.FindEntityType(item.GetType()).FindPrimaryKey().Properties.Select(p => string.Format(p.FindAnnotation("KeyNameFormat") != null ? "{0:" + p.FindAnnotation("KeyNameFormat")?.Value.ToString() + "}" : "{0}", p.PropertyInfo.GetValue(item, null))));
        }
    }
}
