using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Widgets.ProductPictureModifier.Domain;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Services
{
    /// <summary>
    /// Replaces ProductModelFactory of Nop.Web to incorporate the picture with logo and color in the product box
    /// </summary>
    public class ProductPictureModifierProductModelFactory : ProductModelFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly IProductPictureModifierService _productPictureModifierService;
        private readonly ILogoPositionService _logoPositionService;

        #region Ctor

        public ProductPictureModifierProductModelFactory(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IReviewTypeService reviewTypeService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            SeoSettings seoSettings,
            VendorSettings vendorSettings,
            IProductPictureModifierService productPictureModifierService,
            ILogoPositionService logoPositionService) : base(
             captchaSettings,
             catalogSettings,
             customerSettings,
             categoryService,
             currencyService,
             customerService,
             dateRangeService,
             dateTimeHelper,
             downloadService,
             localizationService,
             manufacturerService,
             permissionService,
             pictureService,
             priceCalculationService,
             priceFormatter,
             productAttributeParser,
             productAttributeService,
             productService,
             productTagService,
             productTemplateService,
             reviewTypeService,
             specificationAttributeService,
             cacheManager,
             storeContext,
             taxService,
             urlRecordService,
             vendorService,
             webHelper,
             workContext,
             mediaSettings,
             orderSettings,
             seoSettings,
             vendorSettings
            )
        {
            _localizationService = localizationService;
            _pictureService = pictureService;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _productPictureModifierService = productPictureModifierService;
            _logoPositionService = logoPositionService;
        }

        #endregion


        /// <summary>
        /// Prepare the product overview picture model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>Picture model</returns>
        protected override PictureModel PrepareProductOverviewPictureModel(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = _localizationService.GetLocalized(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY,
                product.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(),
                _storeContext.CurrentStore.Id);

            var defaultPictureModel = _cacheManager.Get(cacheKey, () =>
            {
                Picture picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                ProductPicture firstTransparentPictureWithLogo = _productPictureModifierService
                    .GetFirstProductPictureWithLogo(product.Id);

                LogoPosition logoPosition = _logoPositionService.GetByProductId(product.Id);
                var colorDictionary = new Dictionary<string, object> { ["Color"] = logoPosition?.ColorCode };
                var pictureModel = new PictureModel
                {
                    CustomProperties = colorDictionary,
                    ImageUrl = _pictureService.GetPictureUrl(firstTransparentPictureWithLogo?.Picture ?? picture, pictureSize),
                    FullSizeImageUrl = _pictureService.GetPictureUrl(firstTransparentPictureWithLogo?.Picture ?? picture),
                    //"title" attribute
                    Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                        ? picture.TitleAttribute
                        : string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                        ? picture.AltAttribute
                        : string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            return defaultPictureModel;
        }
    }
}
