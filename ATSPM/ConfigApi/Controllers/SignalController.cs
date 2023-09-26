using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Google.Apis.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using NetTopologySuite.Densify;
using System;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    [AutoExpand]
    [ApiVersion(1.0)]
    public class SignalController : AtspmConfigControllerBase<Signal, int>
    {
        private readonly ISignalRepository _repository;

        public SignalController(ISignalRepository repository) : base(repository)
        {
            _repository = repository;

        }

        [EnableQuery]
        public IActionResult GetApproaches([FromRoute] int key)
        {
            var approaches = _repository.GetList().Where(w => w.Id == key).SelectMany(m => m.Approaches);

            if (approaches == null)
            {
                return NotFound();
            }

            return Ok(approaches);
        }


        /// <summary>
        /// Get all active <see cref="Signal"/> that match <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">Signal controller identifier</param>
        /// <param name="options">oData query options</param>
        /// <returns>List of <see cref="Signal"/> in decescing order of start date</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Signal>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetAllVersionsOfSignal(string identifier, ODataQueryOptions<Signal> options)
        {
            return Ok(options.ApplyTo(_repository.GetAllVersionsOfSignal(identifier).AsQueryable()));
        }











        //GET THIS WORKING WITHOUT THE QUERY ATTRIBUTE!!!!


        /// <summary>
        /// Get latest version of all <see cref="Signal"/>
        /// </summary>
        /// <param name="options">oData query options</param>
        /// <returns>List of <see cref="Signal"/> with newest start date</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Signal>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        [EnableQuery]
        public IActionResult GetLatestVersionOfAllSignals()
        {
            //return Ok(options.ApplyTo(_repository.GetLatestVersionOfAllSignals().AsQueryable()));

            return Ok(_repository.GetLatestVersionOfAllSignals());
        }























        /// <summary>
        /// Get latest version of <see cref="Signal"/> and related entities that match <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">Signal controller identifier</param>
        /// <param name="options">oData query options</param>
        /// <returns>Lastest <see cref="Signal"/> version</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Signal), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult GetLatestVersionOfSignal(string identifier, ODataQueryOptions<Signal> options)
        {
            //https://learn.microsoft.com/en-us/odata/webapi-8/fundamentals/query-options?tabs=net60
            options.Request.ODataFeature().SelectExpandClause = new SelectExpandQueryOption(null, "Approaches", options.Context, new ODataQueryOptionParser(
                model: options.Context.Model,
                targetEdmType: options.Context.NavigationSource.EntityType(),
                targetNavigationSource: options.Context.NavigationSource,
                queryOptions: new Dictionary<string, string>
                {
                    { "$expand", "Approaches($expand=Detectors), Jurisdiction, ControllerType, Region, VersionAction, Areas" }
                },
                container: options.Context.RequestContainer)).SelectExpandClause;

            var i = _repository.GetLatestVersionOfSignal(identifier);

            if (i == null)
            {
                return NotFound(identifier);
            }

            return Ok(i);
        }

        /// <summary>
        /// Copies <see cref="Signal"/> and associated <see cref="Approach"/> to new version
        /// </summary>
        /// <param name="key">Signal version to copy</param>
        /// <returns>New version of copied <see cref="Signal"/></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Signal), Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CopySignalToNewVersion(int key)
        {
            try
            {
                return Ok(await _repository.CopySignalToNewVersion(key));
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
        }

        /// <summary>
        /// Marks <see cref="Signal"/> to deleted
        /// </summary>
        /// <param name="key">Key of <see cref="Signal"/> to mark as deleted</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetSignalToDeleted(int key)
        {
            try
            {
                await _repository.SetSignalToDeleted(key);
            }
            catch (ArgumentException e)
            {
                return NotFound(e.Message);
            }
            
            return Ok();
        }












        [HttpGet]
        [ProducesResponseType(typeof(Signal), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult GetSignalsForMetricType(int metricTypeId, ODataQueryOptions<Signal> options)
        {
            var i = _repository.GetSignalsForMetricType(metricTypeId);

            return Ok(options.ApplyTo(i.AsQueryable()));
        }
    }
}
