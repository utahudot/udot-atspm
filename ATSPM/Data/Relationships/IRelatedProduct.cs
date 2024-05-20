using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related product
    /// </summary>
    public interface IRelatedProduct
    {
        /// <summary>
        /// Product Id
        /// </summary>
        int? ProductId { get; set; }
        
        /// <summary>
        /// Product
        /// </summary>
        Product? Product { get; set; }
    }
}
