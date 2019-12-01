using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Media;
using Nop.Plugin.Widgets.ProductPictureModifier.Domain;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Web.Infrastructure.Cache;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Controllers
{
    //todo product filter based on vendor and custom view engine to replace the long view path
    /// <summary>
    /// Product Picture Modifier's controller
    /// </summary>
    [Area(AreaNames.Admin)]
    public class WidgetsProductPictureModifierController : BasePluginController
    {
        #region Field

        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductPictureModifierService _productPictureModifierService;
        private readonly IProductService _productService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ICustomLogoService _logoService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ISettingService _settingService;
        private readonly MediaSettings _mediaSettings;
        private readonly ILogoPositionService _logoPositionService;
        private readonly IStaticCacheManager _cacheManager;

        #endregion Field

        #region Constructor

        public WidgetsProductPictureModifierController(
            IPermissionService permissionService,
            IProductPictureModifierService productPictureModifierService,
            IProductAttributeService productAttributeService,
            IProductService productService,
            ILocalizationService localizationService,
            ICategoryModelFactory categoryModelFactory,
            IPictureService pictureService,
            ICustomLogoService logoService,
            IProductAttributeParser productAttributeParser,
            ISettingService settingService,
            MediaSettings mediaSettings,
            ILogoPositionService logoPositionService,
            IStaticCacheManager cacheManager)
        {
            _permissionService = permissionService;
            _productPictureModifierService = productPictureModifierService;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _localizationService = localizationService;
            _categoryModelFactory = categoryModelFactory;
            _pictureService = pictureService;
            _logoService = logoService;
            _productAttributeParser = productAttributeParser;
            _settingService = settingService;
            _mediaSettings = mediaSettings;
            _logoPositionService = logoPositionService;
            _cacheManager = cacheManager;
        }

        #endregion Constructor

        #region Utilities

        protected Dictionary<string, object> GetErrorsFromModelState()
        {
            var errors = new Dictionary<string, object>();
            foreach (var key in ModelState.Keys)
            {
                // Only send the errors to the client.
                if (ModelState[key].Errors.Count > 0)
                {
                    errors[$"{nameof(LogoModel)}.{key}"] = ModelState[key].Errors;
                }
            }
            return errors;
        }

        protected bool IsLogoSettingNew(LogoPosition existingLogoSetting)
        {
            return existingLogoSetting.Opacity == 0 && existingLogoSetting.Size == 0 &&
                   existingLogoSetting.XCoordinate == 0 && existingLogoSetting.YCoordinate == 0;
        }

        #endregion Utilities

        #region Methods

        #region Listing Page

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var searchModel = new ProductPictureModifierSearchModel();
            //prepare page parameters
            searchModel.SetGridPageSize();

            return View("~/Plugins/Widgets.ProductPictureModifier/Views/Admin/List.cshtml", searchModel);
        }

        [HttpPost]
        public IActionResult List(ProductPictureModifierSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            ProductAttribute productAttribute = _productPictureModifierService.
                GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeName).First();

            IPagedList<Product> products = _productService.
                GetProductsByProductAtributeId(productAttribute.Id, searchModel.Page - 1, searchModel.PageSize);

            var model = new ProductPictureModifierListModel()
            {
                Data = products.Select(product => new ProductPictureModifierModel
                {
                    EntityId = product.ProductAttributeMappings
                        .First(x => x.ProductAttributeId == productAttribute.Id).Id,
                    ProductName = product.Name,
                    ProductId = product.Id,
                    ColorCode = product.ProductAttributeMappings
                        .First(x => x.ProductAttributeId == productAttribute.Id).DefaultValue
                }),
                Total = products.TotalCount
            };

            return Json(model);
        }

        #endregion Listing Page

        #region Custom Color

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var model = new ProductPictureModifierModel()
            {
                ColorCode = ProductPictureModifierDefault.DefaultColorCode
            };
            return View("~/Plugins/Widgets.ProductPictureModifier/Views/Admin/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ProductPictureModifierModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //todo find out why model state validation failed even though product id is only validated and model has product id
            if (model.ProductId < 1)
            {
                ModelState.AddModelError(nameof(model.ProductId),
                    _localizationService.GetResource("Admin.Widgets.ProductPictureModifier.Fields.Product.Required"));
                return View("~/Plugins/Widgets.ProductPictureModifier/Views/Admin/Create.cshtml", model);
            }

            ProductAttribute productAttribute = _productPictureModifierService.
                GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeName).First();

            var productAttributeMapping = new ProductAttributeMapping
            {
                ProductAttributeId = productAttribute.Id,
                ProductId = model.ProductId,
                DefaultValue = model.ColorCode,
                AttributeControlTypeId = (int)AttributeControlType.TextBox
            };
            _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);

            //Logo Default Value
            _logoPositionService.Insert(new LogoPosition
            {
                ProductId = model.ProductId,
                ColorCode = model.ColorCode
            });
            //Clear the cache for product picture preparation 
            _cacheManager.RemoveByPattern(string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_PATTERN_KEY_BY_ID, model.ProductId));

            SuccessNotification(_localizationService.GetResource("Widgets.ProductPictureModifier.Mapping.Created"));

            if (!continueEditing)
                return RedirectToAction("Configure");

            return RedirectToAction("Edit", new { id = productAttributeMapping.Id });
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            ProductAttributeMapping productAttributeMapping = _productAttributeService.
                GetProductAttributeMappingById(id);
            //_mediaSettings.ProductDetailsPictureSiz
            ProductPicture defaultProductPicture = _productPictureModifierService.GetFirstProductPicture(productAttributeMapping.ProductId);

            //Existing logoSetting
            LogoPosition existingLogoSetting = _logoPositionService.GetByProductId(productAttributeMapping.ProductId);

            var model = new ProductPictureModifierModel
            {
                CustomImagePath = defaultProductPicture == null ? "" : _pictureService.GetPictureUrl(defaultProductPicture.Picture),
                EntityId = productAttributeMapping.Id,
                ProductId = productAttributeMapping.ProductId,
                ProductName = productAttributeMapping.Product.Name,
                ColorCode = productAttributeMapping.DefaultValue,
                LogoModel = new LogoModel()
                {
                    Opacity = IsLogoSettingNew(existingLogoSetting) ? ProductPictureModifierDefault.LogoOpacity : existingLogoSetting.Opacity,
                    Size = IsLogoSettingNew(existingLogoSetting) ? ProductPictureModifierDefault.LogoSizeInPercent : existingLogoSetting.Size,
                    XCoordinate = existingLogoSetting.XCoordinate,
                    YCoordinate = existingLogoSetting.YCoordinate,
                    MarkAsDefault = IsLogoSettingNew(existingLogoSetting)
                }
            };
            model.LogoSearchModel.SetGridPageSize();

            return View("~/Plugins/Widgets.ProductPictureModifier/Views/Admin/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ProductPictureModifierModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //todo find out why model state validation failed even though product id is only validated and model has product id
            if (model.ProductId < 1)
            {
                ModelState.AddModelError(nameof(model.ProductId),
                    _localizationService.GetResource("Admin.Widgets.ProductPictureModifier.Fields.Product.Required"));
                return View("~/Plugins/Widgets.ProductPictureModifier/Views/Admin/Edit.cshtml", model);
            }

            ProductAttributeMapping productAttributeMapping = _productAttributeService.
                GetProductAttributeMappingById(model.EntityId);

            productAttributeMapping.DefaultValue = model.ColorCode;
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

            //Update logo setting
            LogoPosition existingLogoSetting = _logoPositionService.GetByProductId(productAttributeMapping.ProductId);
            existingLogoSetting.ColorCode = model.ColorCode;
            _logoPositionService.Update(existingLogoSetting);

            //Clear the cache for product picture preparation 
            _cacheManager.RemoveByPattern(string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_PATTERN_KEY_BY_ID, model.ProductId));

            SuccessNotification(_localizationService.GetResource("Widgets.ProductPictureModifier.Mapping.Updated"));

            if (!continueEditing)
                return RedirectToAction("Configure");

            return RedirectToAction("Edit", new { id = productAttributeMapping.Id });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            IList<ProductAttributeMapping> mappings = _productAttributeService
                .GetProductAttributeMappingsByProductId(id);

            var allAttributes = _productPictureModifierService.GetProductAttributeUsedByPlugin();

            //remove product attribute mapping
            foreach (var mapping in mappings)
            {
                if (allAttributes.Any(x => x.Id == mapping.ProductAttributeId))
                    _productAttributeService.DeleteProductAttributeMapping(mapping);
            }

            //remove product picture with logo
            _productPictureModifierService.RemovePictureCreatedByPluginForProduct(id);

            //remove logo image's setting
            _logoPositionService.DeleteByProductId(id);

            SuccessNotification(_localizationService.GetResource("Widgets.ProductPictureModifier.Mapping.Deleted"));
            return RedirectToAction("Configure");
        }

        public virtual IActionResult ProductAddPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //prepare model
            AddProductToCategorySearchModel model = _categoryModelFactory.
                PrepareAddProductToCategorySearchModel(new AddProductToCategorySearchModel());

            return View("~/Plugins/Widgets.ProductPictureModifier/Views/Admin/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult ProductAddPopupList(AddProductToCategorySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            ProductAttribute productAttribute = _productPictureModifierService.
                GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeName).First();

            IPagedList<Product> products = _productPictureModifierService.SearchProducts(
                    searchModel.SearchProductName,
                    searchModel.SearchCategoryId,
                    productAttribute.Id,
                    searchModel.Page - 1,
                    searchModel.PageSize);

            var model = new DataSourceResult
            {
                Data = products.Select(product => product.ToModel<ProductModel>()),
                Total = products.TotalCount
            };

            return Json(model);
        }

        #endregion Custom Color

        #region Custom Logo

        [HttpPost]
        public virtual IActionResult CustomLogoList(ProductPictureModifierSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedKendoGridJson();

            //try to get a product with the specified id
            Product product = _productService.GetProductById(searchModel.ProductId)
                          ?? throw new ArgumentException("No product found with the specified id");

            ProductAttribute predefinedLogoAttribute = _productPictureModifierService
                                                    .GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogo)
                                                    .FirstOrDefault() ?? throw new ArgumentException("Product Attribute not found");

            ProductAttributeMapping predefinedLogoAttributeMapping = _productAttributeService
                                                                  .GetProductAttributeMappingsByProductId(searchModel.ProductId)
                                                                  .FirstOrDefault(x => x.ProductAttributeId == predefinedLogoAttribute.Id);

            if (predefinedLogoAttributeMapping == null)
                return Json(new ProductAttributeValueListModel());

            //get product attribute values
            IList<ProductAttributeValue> productAttributeValues = _productAttributeService
                                                    .GetProductAttributeValues(predefinedLogoAttributeMapping.Id);

            //prepare list model
            var model = new ProductAttributeValueListModel
            {
                Data = productAttributeValues.PaginationByRequestModel(searchModel).Select(value =>
                {
                    //fill in model values from the entity
                    var productAttributeValueModel = new ProductAttributeValueModelForLogo()
                    {
                        Id = value.Id,
                        ProductAttributeMappingId = value.ProductAttributeMappingId,
                        ImageSquaresPictureId = value.ImageSquaresPictureId,
                        PictureId = value.PictureId,
                        Name = value.Name
                    };

                    var pictureThumbnailUrl = _pictureService.GetPictureUrl(value.PictureId, 150, false);
                    //little hack here. Grid is rendered wrong way with <img> without "src" attribute
                    if (string.IsNullOrEmpty(pictureThumbnailUrl))
                        pictureThumbnailUrl = _pictureService.GetPictureUrl(null, 1);
                    productAttributeValueModel.PictureThumbnailUrl = pictureThumbnailUrl;

                    var logoThumbnailUrl = _pictureService.GetPictureUrl(value.ImageSquaresPictureId, 50, false);
                    if (string.IsNullOrEmpty(logoThumbnailUrl))
                        logoThumbnailUrl = _pictureService.GetPictureUrl(null, 1);
                    productAttributeValueModel.LogoThumbnailUrl = logoThumbnailUrl;

                    return productAttributeValueModel;
                }),
                Total = productAttributeValues.Count
            };

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult CustomLogoDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute value with the specified id
            var productAttributeValue = _productAttributeService.GetProductAttributeValueById(id)
                                        ?? throw new ArgumentException("No product attribute value found with the specified id");

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = _productAttributeService.GetProductAttributeMappingById(productAttributeValue.ProductAttributeMappingId)
                                          ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = _productService.GetProductById(productAttributeMapping.ProductId)
                          ?? throw new ArgumentException("No product found with the specified id");


            _productAttributeService.DeleteProductAttributeValue(productAttributeValue);

            //Delete product picture after removing the product attribute
            ProductPicture productPicture = _productService.GetProductPicturesByProductId(product.Id)
                .FirstOrDefault(x => x.PictureId == productAttributeValue.PictureId);

            if (productPicture != null)
                _productService.DeleteProductPicture(productPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual IActionResult CustomLogoAdd(LogoModel logo)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Json(new { Result = false, Errors = GetErrorsFromModelState() });

            //Get 1st product picture we use 1st picture of product to combine with logo
            ProductPicture productPicture = _productPictureModifierService
                .GetFirstProductPicture(logo.ProductId);
            if (productPicture == null)
                throw new Exception("Product does not have any image");

            byte[] logoBinary = _pictureService.LoadPictureBinary(_pictureService.GetPictureById(logo.PictureId));

            //Get new product image which has been merged with the logo uploaded
            (int, string) mergedPicture = _logoService.MergeProductPictureWithLogo(
                logoBinary,
                logo.PictureId,
                logo.ProductId,
                ProductPictureModifierUploadType.Predefined,
                logo.Size,
                logo.Opacity,
                logo.XCoordinate,
                logo.YCoordinate,
                overrideThumb: true);

            if (logo.AttributeValueId > 0)
            {
                var existingAttributeValue = _productAttributeService.
                    GetProductAttributeValueById(logo.AttributeValueId);

                var existingProductPicture = _productService.GetProductPicturesByProductId(logo.ProductId)
                      .First(x => x.PictureId == existingAttributeValue.PictureId);

                //update product attribute value
                existingAttributeValue.PictureId = mergedPicture.Item1;
                existingAttributeValue.ImageSquaresPictureId = logo.PictureId;
                _productAttributeService.UpdateProductAttributeValue(existingAttributeValue);

                //update product picture
                existingProductPicture.PictureId = mergedPicture.Item1;
                _productService.UpdateProductPicture(existingProductPicture);

                return Json(new { Result = true, AttributeId = logo.AttributeValueId });
            }
            //Insert the image as product picture
            _productService.InsertProductPicture(new ProductPicture
            {
                ProductId = logo.ProductId,
                PictureId = mergedPicture.Item1,
                DisplayOrder = ProductPictureModifierDefault.MergedPictureDisplayOrder
            });

            ProductAttribute logoAttribute = _productPictureModifierService
                .GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogo)
                .FirstOrDefault() ?? throw new ArgumentException("Product Attribute Not Found");

            //get product's attribute for predefined logo attributes
            ProductAttributeMapping logoProductMapping = _productAttributeService
                .GetProductAttributeMappingsByProductId(logo.ProductId)
                .FirstOrDefault(x => x.ProductAttributeId == logoAttribute.Id);

            //create the mapping if it does not exist
            if (logoProductMapping == null)
            {
                logoProductMapping = new ProductAttributeMapping
                {
                    ProductAttributeId = logoAttribute.Id,
                    ProductId = logo.ProductId,
                    AttributeControlTypeId = (int)AttributeControlType.ImageSquares

                };
                _productAttributeService.InsertProductAttributeMapping(logoProductMapping);

                ////no logo attribute
                ////todo find a way to use the picture for this
                //_productAttributeService.InsertProductAttributeValue(new ProductAttributeValue
                //{
                //    ProductAttributeMappingId = logoProductMapping.Id,
                //    AttributeValueType = AttributeValueType.Simple,
                //    Name = _localizationService.GetResource("Widgets.ProductPictureModifier.Attributes.NoLogo"),
                //    ImageSquaresPictureId = 1,
                //    PictureId = productPicture.PictureId,
                //});

                //provision for manual upload by user
                Setting customUploadIconSetting = _settingService
                    .GetSetting(ProductPictureModifierDefault.UploadIconPictureIdSetting);
                var customUploadForLogoAttribute = new ProductAttributeValue
                {
                    ProductAttributeMappingId = logoProductMapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = _localizationService.GetResource("Widgets.ProductPictureModifier.Attributes.Upload"),
                    ImageSquaresPictureId = int.Parse(customUploadIconSetting.Value),
                };
                _productAttributeService.InsertProductAttributeValue(customUploadForLogoAttribute);

                ProductAttribute productAttributeForCustomUpload = _productPictureModifierService
                                 .GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogoUpload)
                                 .FirstOrDefault() ?? throw new ArgumentException("Product Attribute Not Found for Custom Upload");

                //custom upload attribute mapping with product based on condition
                var customUploadProductAttributeMapping = new ProductAttributeMapping
                {
                    ProductAttributeId = productAttributeForCustomUpload.Id,
                    ProductId = logo.ProductId,
                    AttributeControlTypeId = (int)AttributeControlType.FileUpload,
                    ValidationFileAllowedExtensions = "png",
                    ConditionAttributeXml = _productAttributeParser.AddProductAttribute(null, logoProductMapping,
                        customUploadForLogoAttribute.Id.ToString())
                };
                _productAttributeService.InsertProductAttributeMapping(customUploadProductAttributeMapping);

            }

            //save the actual logo attribute
            var productAttributeValue = new ProductAttributeValue
            {
                ProductAttributeMappingId = logoProductMapping.Id,
                AttributeValueType = AttributeValueType.Simple,
                Name = "Custom Logo",
                ImageSquaresPictureId = logo.PictureId,
                PictureId = mergedPicture.Item1,
            };
            _productAttributeService.InsertProductAttributeValue(productAttributeValue);

            //Logo Position for custom upload
            LogoPosition logoPosition = _logoPositionService.GetByProductId(logo.ProductId);
            if (logo.MarkAsDefault || IsLogoSettingNew(logoPosition))
            {
                logoPosition.Size = logo.Size;
                logoPosition.XCoordinate = logo.XCoordinate;
                logoPosition.YCoordinate = logo.YCoordinate;
                logoPosition.Opacity = logo.Opacity;
                _logoPositionService.Update(logoPosition);
            }

            return Json(new { Result = true, AttributeId = productAttributeValue.Id });
        }

        #endregion Custom Logo

        #endregion Methods
    }
}
