using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Common.EntityTable
{
    public class RouteEntityTable
    {
        public int RouteId { get; set; }
        public int EntityId { get; set; }
        public string EntityType { get; set; }
        public string SourceId { get; set; }
    }
}
