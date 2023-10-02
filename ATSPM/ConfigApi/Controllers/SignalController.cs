using Asp.Versioning;
using Asp.Versioning.OData;
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Application.Specifications;
using ATSPM.ConfigApi.Models;
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
using ATSPM.Domain.Extensions;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using Microsoft.EntityFrameworkCore;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Signal Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class SignalController : AtspmConfigControllerBase<Signal, int>
    {
        private readonly ISignalRepository _repository;

        /// <inheritdoc/>
        public SignalController(ISignalRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// Gets all <see cref="Approach"/> navigation properties
        /// </summary>
        /// <param name="key">Signal key</param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Approach>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult GetApproaches([FromRoute] int key)
        {
            var result = _repository.GetList().Include(i => i.Approaches).FirstOrDefault(f => f.Id == key);

            if (result == null)
            {
                return NotFound(key);
            }

            return Ok(result.Approaches);
        }

        /// <summary>
        /// Gets all <see cref="Area"/> navigation properties
        /// </summary>
        /// <param name="key">Signal key</param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Area>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult GetAreas([FromRoute] int key)
        {
            var result = _repository.GetList().Include(i => i.Areas).FirstOrDefault(f => f.Id == key);

            if (result == null)
            {
                return NotFound(key);
            }

            return Ok(result.Areas);
        }

        #endregion

        #region Actions

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

        #endregion

        #region Functions

        /// <summary>
        /// Get latest version of <see cref="Signal"/> and related entities that match <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">Signal controller identifier</param>
        /// <returns>Lastest <see cref="Signal"/> version</returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Expand | Select)]
        [ProducesResponseType(typeof(Signal), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        public IActionResult GetLatestVersionOfSignal(string identifier)
        {
            //https://learn.microsoft.com/en-us/odata/webapi-8/fundamentals/query-options?tabs=net60
            //options.Request.ODataFeature().SelectExpandClause = new SelectExpandQueryOption(null, "Approaches", options.Context, new ODataQueryOptionParser(
            //    model: options.Context.Model,
            //    targetEdmType: options.Context.NavigationSource.EntityType(),
            //    targetNavigationSource: options.Context.NavigationSource,
            //    queryOptions: new Dictionary<string, string>
            //    {
            //        { "$expand", "Approaches, Areas" }
            //    },
            //    container: options.Context.RequestContainer)).SelectExpandClause;

            var result = _repository.GetLatestVersionOfSignal(identifier);

            if (result == null)
            {
                return NotFound(identifier);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all active <see cref="Signal"/> that match <paramref name="identifier"/>
        /// </summary>
        /// <param name="identifier">Signal controller identifier</param>
        /// <param name="options">oData query options</param>
        /// <returns>List of <see cref="Signal"/> in decescing order of start date</returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Signal>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetAllVersionsOfSignal(string identifier)
        {
            var result = _repository.GetAllVersionsOfSignal(identifier);

            if (!result.Any())
            {
                return NotFound(identifier);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get latest version of all <see cref="Signal"/>
        /// </summary>
        /// <returns>List of <see cref="Signal"/> with newest start date</returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<Signal>), Status200OK)]
        public IActionResult GetLatestVersionOfAllSignals()
        {
            return Ok(_repository.GetLatestVersionOfAllSignals());
        }

        /// <summary>
        /// Gets an optimized list of <see cref="SearchSignal"/> to use for signal selection
        /// </summary>
        /// <param name="areaId">Signals by area</param>
        /// <param name="regionId">Signals by region</param>
        /// <param name="jurisdictionId">Signals by jurisdiction</param>
        /// <param name="metricTypeId">Signals by chart type</param>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = Count | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(typeof(IEnumerable<SearchSignal>), Status200OK)]
        [ProducesResponseType(Status400BadRequest)]
        public IActionResult GetSignalsForSearch([FromQuery] int? areaId, [FromQuery] int? regionId, [FromQuery] int? jurisdictionId, [FromQuery] int? metricTypeId)
        {
            var result = _repository.GetList()
                .FromSpecification(new ActiveSignalSpecification())

                .Where(w => jurisdictionId == null || w.JurisdictionId == jurisdictionId)
                .Where(w => regionId == null || w.RegionId == regionId)
                .Where(w => areaId == null || w.Areas.Any(a => a.Id == areaId))
                .Where(w => metricTypeId == null || w.Approaches.Any(m => m.Detectors.Any(d => d.DetectionTypes.Any(t => t.MetricTypeMetrics.Any(a => a.Id == metricTypeId)))))

                .Select(s => new SearchSignal()
                {
                    Id = s.Id,
                    Start = s.Start,
                    SignalIdentifier = s.SignalIdentifier,
                    PrimaryName = s.PrimaryName,
                    SecondaryName = s.SecondaryName,
                    RegionId = s.RegionId,
                    JurisdictionId = s.JurisdictionId,
                    Longitude = s.Longitude,
                    Latitude = s.Latitude,
                    ChartEnabled = s.ChartEnabled,
                    Areas = s.Areas.Select(a => a.Id),
                    Charts = s.Approaches.SelectMany(m => m.Detectors.SelectMany(d => d.DetectionTypes.SelectMany(t => t.MetricTypeMetrics.Select(i => i.Id))))
                })

                .GroupBy(r => r.SignalIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            return Ok(result);
        }

        #endregion
    }
}
