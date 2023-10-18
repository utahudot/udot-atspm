using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    public class MetricTypeController : AtspmConfigControllerBase<MetricType, int>
    {
        private readonly IMetricTypeRepository _repository;

        public MetricTypeController(IMetricTypeRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        //[EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        //[ProducesResponseType(typeof(IEnumerable<ActionLog>), Status200OK)]
        //[ProducesResponseType(Status404NotFound)]
        //public IActionResult GetActionLogs([FromRoute] int key)
        //{
        //    var test = HttpContext.Request.GetEncodedUrl();

        //    var collection = new Uri(test).Segments.Last();

        //    var result = _repository.GetList().Include(collection).FirstOrDefault(f => f.Id == key);

        //    if (result == null)
        //    {
        //        return NotFound(key);
        //    }

        //    return Ok(result.Signals);
        //}

        #endregion
    }
}
