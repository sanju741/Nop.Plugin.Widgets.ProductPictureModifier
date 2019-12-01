using Nop.Core;
using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using Nop.Core.Domain.Media;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Services
{
    /// <summary>
    /// Interface for Product Attribute Service
    /// </summary>
    public interface IProductPictureModifierService
    {
        /// <summary>
        /// Gets product attribute by attribute name
        /// </summary>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>Product attributes </returns>
        IEnumerable<ProductAttribute> GetProductAttributeByName(string attributeName);

        /// <summary>
        /// Gets all Product Attributes created by this plugin
        /// </summary>
        /// <returns>Product Attributes</returns>
        IList<ProductAttribute> GetProductAttributeUsedByPlugin();

        /// <summary>
        /// Search products 
        /// </summary>
        /// <param name="keyword">Keyword identifier</param>
        /// <param name="categoryId">Category Id. </param>
        /// <param name="ignoredProductAttributesId">Product Attribute Id to exclude products </param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Products</returns>
        IPagedList<Product> SearchProducts(
           string keyword,
           int categoryId = 0,
           int ignoredProductAttributesId = 0,
           int pageIndex = 0,
           int pageSize = int.MaxValue - 1);

        /// <summary>
        /// Gets first picture of product (Ordered by display order followed by id in ascending manner)
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>ProductPicture</returns>
        ProductPicture GetFirstProductPicture(int productId);

        /// <summary>
        /// Gets download from the attribute-xml
        /// </summary>
        /// <param name="customUploadAttribute">custom upload attribute's value</param>
        /// <returns>Download</returns>
        Download GetDownloadFromAttributeXml(string customUploadAttribute);

        /// <summary>
        /// Gets the 1st product picture used with logo
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        ProductPicture GetFirstProductPictureWithLogo(int productId);

        /// <summary>
        /// Removes all the picture created by plugin
        /// </summary>
        void RemovePictureCreatedByPlugin();

        /// <summary>
        /// Removes all picture for particular product created by the plugin
        /// </summary>
        /// <param name="productId">Product identifier</param>
        void RemovePictureCreatedByPluginForProduct(int productId);
    }
}
