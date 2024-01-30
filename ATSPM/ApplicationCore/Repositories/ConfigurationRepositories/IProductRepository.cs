using ATSPM.Data.Models;
using ATSPM.Domain.Services;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Products repository
    /// </summary>
    public interface IProductRepository : IAsyncRepository<Product>
    {
    }
}
