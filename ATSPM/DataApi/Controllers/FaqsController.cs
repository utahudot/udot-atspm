using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ATSPM.DataApi.Controllers
{
    public class FaqsController : ODataController
    {
        private readonly ConfigContext _configContext;

        public FaqsController(ConfigContext configContext)
        {
            _configContext = configContext;
        }


        // GET: SignalController
        public IActionResult Get()
        {
            return Ok(_configContext.Faqs);
        }
    }
}
