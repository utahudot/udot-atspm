using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System;
using System.Net;
using System.Reflection.Metadata;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    [ApiVersion(1.0)]
    public class SignalController : AtspmConfigControllerBase<Signal, int>
    {
        private readonly ISignalRepository _repository;

        public SignalController(ISignalRepository repository) : base(repository)
        {
            _repository = repository;

        }

        [HttpGet]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(typeof(IEnumerable<Signal>), Status200OK)]
        [EnableQuery]
        public IActionResult GetLatestVersionOfAllSignals()
        {
            return Ok(_repository.GetLatestVersionOfAllSignals());
        }

        [HttpGet]
        //[ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(typeof(Signal), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery]
        public IActionResult GetLatestVersionOfSignal(string identifier, ODataQueryOptions<Signal> options)
        {
            var i = _repository.GetLatestVersionOfSignal(identifier);

            if (i == null)
            {
                return NotFound(identifier);
            }

            return Ok(i);
        }
    }
}
