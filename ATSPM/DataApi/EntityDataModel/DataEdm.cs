using ATSPM.Data.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Net;

namespace ATSPM.DataApi.EntityDataModel
{
    public class DataEdm
    {
        public IEdmModel GetEntityDataModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AtspmData";
            builder.ContainerName = "AtspmDataContainer";

            var logs = builder.EntityType<ControllerLogArchive>();
            logs.HasKey(k => new { k.SignalId, k.ArchiveDate });

            builder.EntitySet<ControllerLogArchive>("logs");

            builder.EntitySet<Area>("Areas");

            return builder.GetEdmModel();
        }
    }

}
