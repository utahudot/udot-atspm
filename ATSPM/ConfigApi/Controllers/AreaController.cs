﻿using Asp.Versioning;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

namespace ATSPM.ConfigApi.Controllers
{
    /// <summary>
    /// Area Controller
    /// </summary>
    [ApiVersion(1.0)]
    public class AreaController : AtspmConfigControllerBase<Area, int>
    {
        private readonly IAreaRepository _repository;

        /// <inheritdoc/>
        public AreaController(IAreaRepository repository) : base(repository)
        {
            _repository = repository;
        }

        #region NavigationProperties

        /// <summary>
        /// <see cref="Signal"/> navigation property action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [EnableQuery(AllowedQueryOptions = Count | Expand | Filter | Select | OrderBy | Top | Skip)]
        [ProducesResponseType(Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status400BadRequest)]
        public ActionResult<IEnumerable<Signal>> GetSignals([FromRoute] int key)
        {
            return GetNavigationProperty<IEnumerable<Signal>>(key);
        }

        #endregion

        #region Actions

        #endregion

        #region Functions

        #endregion
    }
}