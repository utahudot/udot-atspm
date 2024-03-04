using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;

namespace ATSPM.ConfigApi.Controllers
{
    public class AtspmGeneralConfigBase<T, TKey> : AtspmConfigControllerBase<T, TKey> where T : AtspmConfigModelBase<TKey>
    {
        private readonly IAsyncRepository<T> _repository;
        public AtspmGeneralConfigBase(IAsyncRepository<T> repository) : base(repository)
        {
            _repository = repository;
        }

        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Post([FromBody] T item)
        {
            return base.Post(item);
        }

        [Authorize(Policy = "CanEditGeneralConfigurations")]
        public override Task<IActionResult> Patch(TKey key, [FromBody] Delta<T> item)
        {
            return base.Patch(key, item);
        }

        [Authorize(Policy = "CanDeleteGeneralConfigurations")]
        public override Task<IActionResult> Delete(TKey key)
        {
            return base.Delete(key);
        }
    }
}

