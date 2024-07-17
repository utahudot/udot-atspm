using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Common.Dtos
{
    public class RouteEntityWithSpeed
    {
        public long RouteId { get; set; }
        public long EntityId { get; set; }
        public string EntityType { get; set; }
        public long SourceId { get; set; }
        public long SpeedLimit { get; set; }
    }
}
