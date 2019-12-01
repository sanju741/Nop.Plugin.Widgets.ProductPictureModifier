using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Media;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Services
{
    /// <summary>
    /// Product Attribute Service to be used by the plugin
    /// </summary>
    public class ProductPictureModifierService : IProductPictureModifierService
    {
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly IDownloadService _downloadService;

        public ProductPictureModifierService(IRepository<ProductAttribute> productAttributeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductPicture> productPictureRepository,
            IDownloadService downloadService)
        {
            _productAttributeRepository = productAttributeRepository;
            _productRepository = productRepository;
            _productPictureRepository = productPictureRepository;
            _downloadService = downloadService;
        }

        /// <inheritdoc cref="IProductPictureModifierService.GetProductAttributeByName"/>
        public IEnumerable<ProductAttribute> GetProductAttributeByName(string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException(nameof(attributeName));

            var query = _productAttributeRepository.Table.Where(x => x.Name.Equals(attributeName));
            return query;
        }

        /// <inheritdoc cref="IProductPictureModifierService.GetProductAttributeUsedByPlugin"/>
        public IList<ProductAttribute> GetProductAttributeUsedByPlugin()
        {
            var query = _productAttributeRepository.Table.Where(x =>
                x.Name.Equals(ProductPictureModifierDefault.ProductAttributeName) ||
                x.Name.Equals(ProductPictureModifierDefault.ProductAttributeNameForLogo) ||
                x.Name.Equals(ProductPictureModifierDefault.ProductAttributeNameForLogoUpload));

            return query.ToList();
        }


        /// <inheritdoc cref="IProductPictureModifierService.SearchProducts"/>
        public IPagedList<Product> SearchProducts(
            string keyword,
            int categoryId = 0,
            int ignoredProductAttributesId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue - 1)
        {
            var query = _productRepository.Table.Where(x => !x.Deleted);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Name.Contains(keyword) || x.Sku.Contains(keyword));

            if (categoryId > 0)
                query = query.Where(x => x.ProductCategories
                    .Any(y => y.CategoryId == categoryId));

            if (ignoredProductAttributesId > 0)
                query = query.Where(x => x.ProductAttributeMappings
                    .All(y => y.ProductAttributeId != ignoredProductAttributesId));

            var products = new PagedList<Product>(query, pageIndex, pageSize);
            return products;
        }

        /// <inheritdoc cref="IProductPictureModifierService.GetFirstProductPicture"/>
        public ProductPicture GetFirstProductPicture(int productId)
        {
            var query = from pp in _productPictureRepository.Table
                        where pp.ProductId == productId
                        orderby pp.DisplayOrder, pp.Id
                        select pp;

            var productPictures = query.FirstOrDefault();
            return productPictures;
        }

        /// <inheritdoc cref="IProductPictureModifierService.GetDownloadFromAttributeXml"/>
        public Download GetDownloadFromAttributeXml(string customUploadAttribute)
        {
            int pFrom = customUploadAttribute.
                            IndexOf("download/getfileupload/?downloadId=", StringComparison.Ordinal)
                        + "download/getfileupload/?downloadId=".Length;

            int pTo = customUploadAttribute.IndexOf("\" class=", StringComparison.Ordinal);

            if (pTo <= pFrom || pTo == 0)
                throw new InvalidDataException("Invalid Custom Attribute");

            string downloadGuid = customUploadAttribute.Substring(pFrom, pTo - pFrom);
            return _downloadService.GetDownloadByGuid(new Guid(downloadGuid));
        }

        /// <inheritdoc cref="IProductPictureModifierService.RemovePictureCreatedByPlugin"/>
        public void RemovePictureCreatedByPlugin()
        {
            var query = _productPictureRepository.Table.Where(x => x.DisplayOrder == ProductPictureModifierDefault.MergedPictureDisplayOrder);

            query = query.Where(x => x.Picture.TitleAttribute == ProductPictureModifierDefault.MergedPictureTitle
                                     && x.Picture.AltAttribute == ProductPictureModifierDefault.MergedPictureAlt);

            _productPictureRepository.Delete(query);

        }

        /// <inheritdoc cref="IProductPictureModifierService.GetFirstProductPictureWithLogo"/>
        public ProductPicture GetFirstProductPictureWithLogo(int productId)
        {
            var query = _productPictureRepository.Table.Where(x =>
                x.ProductId == productId &&
                x.DisplayOrder == ProductPictureModifierDefault.MergedPictureDisplayOrder);

            query = query.Where(x => x.Picture.TitleAttribute == ProductPictureModifierDefault.MergedPictureTitle
                                     && x.Picture.AltAttribute == ProductPictureModifierDefault.MergedPictureAlt);

            return query.FirstOrDefault();
        }

        /// <inheritdoc cref="IProductPictureModifierService.RemovePictureCreatedByPluginForProduct"/>
        public void RemovePictureCreatedByPluginForProduct(int productId)
        {
            var query = _productPictureRepository.Table.Where(x =>
                x.ProductId == productId &&
                x.DisplayOrder == ProductPictureModifierDefault.MergedPictureDisplayOrder);

            query = query.Where(x => x.Picture.TitleAttribute == ProductPictureModifierDefault.MergedPictureTitle
                                     && x.Picture.AltAttribute == ProductPictureModifierDefault.MergedPictureAlt);

            _productPictureRepository.Delete(query);

        }
    }
}
