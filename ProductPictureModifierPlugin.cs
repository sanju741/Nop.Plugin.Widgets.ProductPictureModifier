using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using System.Linq;
using Nop.Plugin.Widgets.ProductPictureModifier.Data;

namespace Nop.Plugin.Widgets.ProductPictureModifier
{
    /// <summary>
    /// Picture Modifier Plugin
    /// </summary>
    public class ProductPictureModifierPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region Field

        private readonly IWebHelper _webHelper;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductPictureModifierService _productPictureModifierService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly INopFileProvider _fileProvider;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly ProductPictureModifierObjectContext _context;

        #endregion Field

        #region Constructor

        public ProductPictureModifierPlugin(IWebHelper webHelper,
            IProductAttributeService productAttributeService,
            IProductPictureModifierService productPictureModifierService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            INopFileProvider fileProvider,
            IPictureService pictureService,
            ISettingService settingService,
            ProductPictureModifierObjectContext context)
        {
            _webHelper = webHelper;
            _productAttributeService = productAttributeService;
            _productPictureModifierService = productPictureModifierService;
            _localizationService = localizationService;
            _workContext = workContext;
            _fileProvider = fileProvider;
            _pictureService = pictureService;
            _settingService = settingService;
            _context = context;
        }

        #endregion Constructor

        #region Utilities

        protected Dictionary<string, string> GetLanguageResources()
        {
            return new Dictionary<string, string>
            {
                {"widgets.productpicturemodifier.productname", "Product Name"},
                {"widgets.productpicturemodifier.colorcode", "Default Color Code"},
                {"admin.configuration.widgets.productpicturemodifier.addnew", "Add Product Picture Color Mapping"},
                {"Admin.Configuration.Widgets.ProductPictureModifier.Edit", "Edit Product Picture Color Mapping"},
                {"admin.configuration.widgets.productpicturemodifier.backtolist", "Back to list"},
                {"Widgets.ProductPictureModifier.Admin.Fields.ProductName", "Product Name"},
                {"Widgets.ProductPictureModifier.Admin.Fields.ColorCode", "Default Color Code"},
                {"admin.configuration.widgets.productpicturemodifier.selectproduct", "Select Product"},
                {"admin.configuration.widgets.productpicturemodifier.product.select", "Select"},
                {"admin.configuration.widgets.productpicturemodifier.editmapping", "Edit Product Picture Color Mapping"},
                {"Widgets.ProductPictureModifier.Menu.Title",  "Product Picture Modifier"},
                {"Widgets.ProductPictureModifier.Mapping.Created",  "Mapping Created Successfully!"},
                {"Widgets.ProductPictureModifier.Mapping.Updated",  "Mapping Updated Successfully!"},
                {"Widgets.ProductPictureModifier.Mapping.Deleted",  "Mapping Deleted Successfully!"},
                {"Admin.Widgets.ProductPictureModifier.Fields.Product.Required",  "Please select a product"},
                {"admin.widgets.productpicturemodifier.color.info",  "Color"},
                {"admin.widgets.productpicturemodifier.logo.info",  "Logo"},
                {"admin.widgets.productpicturemodifier.attributes.color.savebeforeedit","You need to save the mapping before you can upload logo for the product."},
                {"admin.widgets.productpicturemodifier.fields.logotype",  "Type"},
                {"admin.widgets.productpicturemodifier.fields.logo",  "Logo"},
                {"admin.widgets.productpicturemodifier.logo.fields.picture",  "Product picture with logo"},
                {"widgets.productpicturemodifier.attributes.upload",  "Upload Logo"},
                {"admin.widgets.productpicturemodifier.logo.addnew",  "Add New"},
                {"Admin.Widgets.ProductPictureModifier.Logo.UpdateButton",  "Update "},
                {"Widgets.ProductPictureModifiers.Fields.Picture",  "Logo"},
                {"Widgets.ProductPictureModifiers.Fields.SizeInPercent",  "Size(percent)"},
                {"Widgets.ProductPictureModifiers.Fields.Opacity",  "Opacity"},
                {"Widgets.ProductPictureModifiers.Fields.Position",  "Position"},
                {"admin.widgets.productpicturemodifier.logo.addbutton",  "Add as New Logo"},
                {"admin.widgets.productpicturemodifier.fields.opacity.invalid",  "Opacity must be between 0 and 100"},
                {"admin.widgets.productpicturemodifier.fields.size.invalid",  "Size must be between 0 and 100"},
                {"Widgets.ProductPictureModifier.PictureInformation",  "The feature best works with image of .png extension." +
                                                                       "</br> Use of image having other extension may result in loss of product image's transparent feature"},
                {"widgets.productpicturemodifiers.fields.selectposition",  "Select Position"},
                {"widgets.productpicturemodifier.positioninformation",  "You can click on image to select the logo's position"},
                {"Widgets.ProductPictureModifiers.Fields.Position.XCoordinate",  "X-Coordinate"},
                {"Widgets.ProductPictureModifiers.Fields.Position.YCoordinate",  "Y-Coordinate"},
                {"Widgets.ProductPictureModifiers.Fields.Mark.SettingAs.Default",  "Use this setting for custom upload"},
                {"Admin.Widgets.ProductPictureModifier.Product.Image.Unavailable",  "Product must have at least a image to use the feature "},

            };
        }

        #endregion Utilities

        #region Methods

        #region Base Plugin Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetsProductPictureModifier/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //install object context
            _context.Install();

            //Insert language resource
            foreach (KeyValuePair<string, string> entry in GetLanguageResources())
                _localizationService.AddOrUpdatePluginLocaleResource(entry.Key, entry.Value);

            //Check for the product attribute custom color. Insert only in case not found
            if (!_productPictureModifierService.GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeName)
                .Any())
            {
                _productAttributeService.InsertProductAttribute(new ProductAttribute
                {
                    Name = ProductPictureModifierDefault.ProductAttributeName,
                    Description = ProductPictureModifierDefault.ProductAttributeDescription
                });
            }

            //Check for the product attribute custom logo. Insert only in case not found
            if (!_productPictureModifierService.GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogo)
                .Any())
            {
                _productAttributeService.InsertProductAttribute(new ProductAttribute
                {
                    Name = ProductPictureModifierDefault.ProductAttributeNameForLogo,
                    Description = ProductPictureModifierDefault.ProductAttributeDescriptionForLogo
                });
            }

            //Check for the product attribute custom logo Upload. Insert only in case not found
            if (!_productPictureModifierService.GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogoUpload)
                .Any())
            {
                _productAttributeService.InsertProductAttribute(new ProductAttribute
                {
                    Name = ProductPictureModifierDefault.ProductAttributeNameForLogoUpload,
                    Description = ProductPictureModifierDefault.ProductAttributeDescriptionForLogoUpload
                });
            }

            //Upload icon for custom logo upload selector
            byte[] customUploadIcon = _fileProvider.
                ReadAllBytes(_fileProvider.MapPath("~/Plugins/Widgets.ProductPictureModifier/Content/Images/Upload.png"));
            Picture image = _pictureService.InsertPicture(customUploadIcon, "image/png", "custom_upload");
            _settingService.SetSetting(ProductPictureModifierDefault.UploadIconPictureIdSetting, image.Id);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //uninstall object context
            _context.Uninstall();

            //Remove language resource
            foreach (KeyValuePair<string, string> entry in GetLanguageResources())
                _localizationService.DeletePluginLocaleResource(entry.Key);

            //Remove the product attributes created by the plugin
            List<ProductAttribute> customColorAttribute = _productPictureModifierService.
                GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeName).ToList();
            foreach (var attribute in customColorAttribute)
                _productAttributeService.DeleteProductAttribute(attribute);

            List<ProductAttribute> customLogoAttribute = _productPictureModifierService.
                GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogo).ToList();
            foreach (var attribute in customLogoAttribute)
                _productAttributeService.DeleteProductAttribute(attribute);

            List<ProductAttribute> customLogoUploadAttribute = _productPictureModifierService.
                           GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeNameForLogoUpload).ToList();
            foreach (var attribute in customLogoUploadAttribute)
                _productAttributeService.DeleteProductAttribute(attribute);

            Setting uploadIconSetting = _settingService
                .GetSetting(ProductPictureModifierDefault.UploadIconPictureIdSetting);

            //remove setting and custom upload icon logo
            if (uploadIconSetting != null)
            {
                //remove picture
                if (int.TryParse(uploadIconSetting.Value, out int pictureId))
                    _pictureService.DeletePicture(_pictureService.GetPictureById(pictureId));

                //remove setting
                _settingService.DeleteSetting(uploadIconSetting);
            }

            //remove picture mapping created by the plugin
            _productPictureModifierService.RemovePictureCreatedByPlugin();

            base.Uninstall();
        }

        #endregion Base Plugin Methods

        #region Widget Methods

        /// <summary>
        /// Gets List of widget zones where the widgets should be rendered
        /// </summary>
        /// <returns>List of widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                PublicWidgetZones.ProductDetailsOverviewTop,
                PublicWidgetZones.OrderSummaryContentBefore,
                AdminWidgetZones.OrderDetailsProductsTop
            };
        }

        /// <summary>
        /// Gets the appropriate view component for the widget
        /// </summary>
        /// <param name="widgetZone">name of the widget zone</param>
        /// <returns>View Component Name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.ProductDetailsOverviewTop)
                return "WidgetsProductPictureModifier";

            if (widgetZone == PublicWidgetZones.OrderSummaryContentBefore)
                return "WidgetsCartProductPictureModifier";

            return "WidgetsAdminOrderPictureModifier";
        }
        #endregion Widget Methods

        #region Admin Menu Method
        /// <summary>
        /// Adds menu item in the admin panel
        /// </summary>
        /// <param name="rootNode"></param>
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "ProductPictureModifierPlugin",
                Title = _localizationService.GetResource("Widgets.ProductPictureModifier.Menu.Title"),
                IconClass = "fa-gears",
                ControllerName = "WidgetsProductPictureModifier",
                ActionName = "Configure",
                Visible = true,
                OpenUrlInNewTab = false,
                RouteValues = new RouteValueDictionary() { { "area", "admin" } }
            };
            rootNode.ChildNodes.Add(menuItem);
        }

        #endregion Admin Menu Method

        #endregion Methods
    }
}
