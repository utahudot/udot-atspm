﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.ConfigurationRepositories/DeviceEFRepository.cs
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

using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IDeviceRepository"/>
    public class DeviceEFRepository : ATSPMRepositoryEFBase<Device>, IDeviceRepository
    {
        /// <inheritdoc/>
        public DeviceEFRepository(ConfigContext db, ILogger<DeviceEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Device> GetList()
        {
            return base.GetList()
                .Include(i => i.Location)
                .Include(i => i.DeviceConfiguration).ThenInclude(i => i.Product);

        }

        #endregion

        #region IDeviceRepository

        /// <inheritdoc/>
        public IReadOnlyList<Device> GetActiveDevicesByLocation(int locationId)
        {
            return GetList().Where(w => w.LocationId == locationId && w.DeviceStatus == DeviceStatus.Active).ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Device> GetActiveDevicesByAllLatestLocations()
        {
            var result = _db.Set<Location>()
                .Include(i => i.Devices).ThenInclude(i => i.DeviceConfiguration).ThenInclude(i => i.Product)
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .AsEnumerable()
                .SelectMany(m => m.Devices)
                .Where(w => w.DeviceStatus == DeviceStatus.Active && w.LoggingEnabled)
                .ToList();

            return result;
        }

        #endregion
    }
}
