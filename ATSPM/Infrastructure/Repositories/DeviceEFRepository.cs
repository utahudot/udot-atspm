using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories
{
    /// <summary>
    /// Device entity framework repository
    /// </summary>
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
