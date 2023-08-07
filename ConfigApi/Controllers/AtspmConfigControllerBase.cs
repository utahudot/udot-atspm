using Asp.Versioning;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

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
    public class AtspmConfigControllerBase<T, TKey> : ODataController where T : ATSPMModelBase
    {
        private readonly IAsyncRepository<T> _repository;

        /// <inheritdoc/>
        public AtspmConfigControllerBase(IAsyncRepository<T> repository)
        {
            //_configContext = configContext;
            _repository = repository;
        }

        /// <summary>
        /// Collection of type from oData query
        /// </summary>
        /// <returns>Action result of type</returns>
        // GET /Entity
        //[HttpGet()]
        [EnableQuery]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IQueryable<T>> Get()
        {
            return Ok(_repository.GetList());
        }

        /// <summary>
        /// Key type from oData query
        /// </summary>
        /// <param name="key">Key value of object to get</param>
        /// <returns>Action result of type</returns>
        // GET /Entity(1)
        //[HttpGet("{key}")]
        [EnableQuery]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<T?>> Get(TKey key)
        {
            var i = await _repository.LookupAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            return Ok(i);
        }

        /// <summary>
        /// Add new object of specified type
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


        // PUT /Entity(1)
        //[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public virtual async Task<IActionResult> Put(TKey key, [FromBody] T item)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var i = await _repository.LookupAsync(key);

        //    if (i == null)
        //    {
        //        return NotFound();
        //    }

        //    item.Id = i.Id;

        //    await _repository.UpdateAsync(item);

        //    return NoContent();
        //}

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

            return NoContent();
        }

        /// <summary>
        /// Delete object of specified type
        /// </summary>
        /// <param name="key">Key value of object to delete</param>
        /// <returns></returns>
        // DELETE /Entity(1)
        //[HttpDelete("{key}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
    }
}
