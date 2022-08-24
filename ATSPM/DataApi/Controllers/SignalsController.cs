using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ATSPM.DataApi.Controllers
{
    public class SignalsController : ODataController
    {
        private readonly ConfigContext _configContext;

        public SignalsController(ConfigContext configContext)
        {
            _configContext = configContext;
        }

        // GET /Signals
        [EnableQuery]
        public ActionResult<IQueryable<Signal>> Get()
        {
            return Ok(_configContext.Set<Signal>());
        }

        // GET /Signals(1)
        public async Task<ActionResult<Signal?>> Get(int key)
        {
            var i = await _configContext.Set<Signal>().FindAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            return Ok(i);
        }

        // GET /Signals(1)Routing.Models.Book
        //public Book GetBook(int key)

        // POST /Signals 
        public async Task<IActionResult> Post([FromBody]Signal item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _configContext.Set<Signal>().AddAsync(item);
            await _configContext.SaveChangesAsync();

            return Created(item);
        }


        // PUT /Signals(1)
        public async Task<IActionResult> Put(int key, [FromBody] Signal item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var i = await _configContext.Set<Signal>().FindAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            item.Id = i.Id;

            _configContext.Entry(i).CurrentValues.SetValues(item);
            await _configContext.SaveChangesAsync();

            return NoContent();
        }

        // PATCH /Signals(1)
        public async Task<IActionResult> Patch(int key, [FromBody] Delta<Signal> item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var i = await _configContext.Set<Signal>().FindAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            item.Patch(i);
            await _configContext.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /Signals(1)
        public async Task<IActionResult> Delete(int key)
        {
            var i = await _configContext.Set<Signal>().FindAsync(key);

            if (i == null)
            {
                return NotFound();
            }

            _configContext.Set<Signal>().Remove(i);
            await _configContext.SaveChangesAsync();

            return NoContent();
        }

        // PUT /Signals(1)Routing.Models.Book
        //public IActionResult PutBook(int key, Book item)

        // PATCH /Signals(1)Routing.Models.Book
        //public IActionResult PatchBook(int key, Delta<Book> item)

        // DELETE /Signals(1)Routing.Models.Book
        //public IActionResult DeleteBook(int key)

        // GET /Signals(1)/Supplier
        //public Supplier GetSupplierFromSignal(int key)

        // GET /Signals(1)Routing.Models.Book/Author
        //public Author GetAuthorFromBook(int key)

        // POST /Signals(1)/Supplier/$ref
        //public HttpResponseMessage CreateLink(int key,
        //    string navigationProperty, [FromBody] Uri link)

        // DELETE /Signals(1)/Supplier/$ref
        //public HttpResponseMessage DeleteLink(int key,
        //    string navigationProperty, [FromBody] Uri link)

        // DELETE /Signals(1)/Parts(1)/$ref
        //public HttpResponseMessage DeleteLink(int key, string relatedKey, string navigationProperty)

        // GET odata/Signals(1)/Name
        // GET odata/Signals(1)/Name/$value
        //public HttpResponseMessage GetNameFromSignal(int key)

        // GET /Signals(1)Routing.Models.Book/Title
        // GET /Signals(1)Routing.Models.Book/Title/$value
        //public HttpResponseMessage GetTitleFromBook(int key)



    }
}
