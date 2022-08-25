using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ATSPM.DataApi.Controllers
{
    public class ATSPMDataControllerBase<T, TKey> : ODataController where T : ATSPMModelBaseTest
    {
        private readonly ConfigContext _configContext;
        private readonly IAsyncRepository<T> _repository;

        public ATSPMDataControllerBase(ConfigContext configContext, IAsyncRepository<T> repository)
        {
            _configContext = configContext;
            _repository = repository;
        }

        // GET /Entity
        [HttpGet()]
        [EnableQuery]
        public ActionResult<IQueryable<T>> Get()
        {
            return Ok(_repository.GetList());
        }

        // GET /Entity(1)
        public virtual async Task<ActionResult<T?>> Get(TKey key)
        {
            var i = await _repository.LookupAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            return Ok(i);
        }

        // POST /Entity 
        public virtual async Task<IActionResult> Post([FromBody]T item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _repository.AddAsync(item);

            return Created(item);
        }


        // PUT /Entity(1)
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

            item.Id = i.Id;

            await _repository.UpdateAsync(item);

            return NoContent();
        }

        // PATCH /Entity(1)
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

        // DELETE /Entity(1)
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
