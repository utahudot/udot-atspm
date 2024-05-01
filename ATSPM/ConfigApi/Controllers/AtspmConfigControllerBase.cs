using ATSPM.Data.Models.ConfigurationModels;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace ATSPM.ConfigApi.Controllers
{

    /// <summary>
    /// Base class for ATSPM Config oData Controllers
    /// </summary>
    /// <typeparam name="T">Type of ATSPM.Data.Model</typeparam>
    /// <typeparam name="TKey">Model Key data type</typeparam>
    //[ApiController]
    //[Route("[controller]")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class AtspmConfigControllerBase<T, TKey> : ODataController where T : AtspmConfigModelBase<TKey>
    {
        private readonly IAsyncRepository<T> _repository;

        /// <inheritdoc/>
        public AtspmConfigControllerBase(IAsyncRepository<T> repository)
        {
            _repository = repository;
        }

        #region oData Actions

        /// <summary>
        /// Collection of objects from oData query.
        /// </summary>
        /// <returns>Action result of type</returns>
        /// <response code="200">Items successfully retrieved.</response>
        // GET /Entity
        //[HttpGet()]
        [EnableQuery(MaxExpansionDepth = 5)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual ActionResult<IQueryable<T>> Get(ODataQueryOptions<T> options)
        {
            return Ok(_repository.GetList());
        }

        /// <summary>
        /// object with key from oData query.
        /// </summary>
        /// <param name="key">Key value of object to get</param>
        /// <returns>Action result of type</returns>
        /// <response code="200">Item was successfully retrieved.</response>
        /// <response code="404">Item does not exist.</response>
        // GET /Entity(1)
        //[HttpGet("{key}")]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select | AllowedQueryOptions.Expand, MaxExpansionDepth = 5)]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual ActionResult<T> Get(TKey key, ODataQueryOptions<T> options)
        {
            var result = _repository.GetList().Where(w => w.Id.Equals(key));

            if (!result.Any())
            {
                return NotFound(key);
            }

            return Ok(SingleResult.Create(result).Queryable);
        }

        /// <summary>
        /// Insert object of specified type
        /// </summary>
        /// <param name="item">Properties of object to add</param>
        /// <returns>Action result with created object</returns>
        // POST /Entity 
        //[HttpPost("{key}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Post([FromBody] T item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _repository.AddAsync(item);

            return Created(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Key value of object to update</param>
        /// <param name="item">Properites to update</param>
        /// <returns></returns>
        //PUT /Entity(1)
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Put(TKey key, [FromBody] T item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var i = await _repository.LookupAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            if (!key.Equals(item.Id))
            {
                return BadRequest();
            }

            //item.Id = i.Id;

            await _repository.UpdateAsync(item);

            return NoContent();
        }

        /// <summary>
        /// Update object of specified type
        /// </summary>
        /// <param name="key">Key value of object to update</param>
        /// <param name="item">Properites to update</param>
        /// <returns>Action result confirming update</returns>
        // PATCH /Entity(1)
        //[HttpPatch("{key}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Patch(TKey key, [FromBody] Delta<T> item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var i = await _repository.LookupAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            item.Patch(i);

            await _repository.UpdateAsync(i);

            return Updated(i);
        }

        /// <summary>
        /// Delete object of specified type
        /// </summary>
        /// <param name="key">Key value of object to delete</param>
        /// <returns></returns>
        // DELETE /Entity(1)
        //[HttpDelete("{key}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(TKey key)
        {
            var i = await _repository.LookupAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            await _repository.RemoveAsync(i);

            return NoContent();
        }

        #endregion

        /// <summary>
        /// Method to help with creating oData navigation property actions
        /// </summary>
        /// <typeparam name="TType">Type that should be returned</typeparam>
        /// <param name="key">Key of object to navigate from</param>
        /// <returns>Object of type <typeparamref name="TType"/></returns>
        protected virtual ActionResult<TType> GetNavigationProperty<TType>(TKey key)
        {
            var collection = new Uri(HttpContext.Request.GetEncodedUrl()).Segments.Last().Capitalize();

            var obj = _repository.GetList().Include(collection).FirstOrDefault(f => f.Id.Equals(key));

            if (obj == null)
                return NotFound(key);

            if (!obj.HasProperty(collection))
                return BadRequest(collection);

            var result = obj.GetPropertyValue<TType>(collection);

            return Ok(result);
        }
    }
}
