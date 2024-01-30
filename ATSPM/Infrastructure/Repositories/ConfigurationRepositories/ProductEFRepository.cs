using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Product entity framework repository
    /// </summary>
    public class ProductEFRepository : ATSPMRepositoryEFBase<Product>, IProductRepository
    {
        /// <inheritdoc/>
        public ProductEFRepository(ConfigContext db, ILogger<ProductEFRepository> log) : base(db, log) { }

        #region Overrides

        #endregion

        #region IProductRepository

        #endregion
    }
}
