using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Framework.Components;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Components
{
    /// <summary>
    /// Cart items picture customization based on the selected attributes
    /// </summary>
    public class WidgetsCartProductPictureModifier : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ICustomLogoService _customLogoService;
        private readonly MediaSettings _mediaSettings;
        private readonly IDownloadService _downloadService;
        private readonly IProductPictureModifierService _productPictureModifierService;
        private readonly ILogoPositionService _logoPositionService;

        public WidgetsCartProductPictureModifier(IWorkContext workContext,
            IProductAttributeFormatter productAttributeFormatter,
            ICustomLogoService customLogoService,
            MediaSettings mediaSettings,
            IDownloadService downloadService,
            IProductPictureModifierService productPictureModifierService,
            ILogoPositionService logoPositionService)
        {
            _workContext = workContext;
            _productAttributeFormatter = productAttributeFormatter;
            _customLogoService = customLogoService;
            _mediaSettings = mediaSettings;
            _downloadService = downloadService;
            _productPictureModifierService = productPictureModifierService;
            _logoPositionService = logoPositionService;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            ICollection<ShoppingCartItem> cartItems = _workContext.CurrentCustomer.ShoppingCartItems;
            var cartPictureModifierModel = new List<ProductPictureModifierModel>();
            foreach (var cart in cartItems)
            {
                if (string.IsNullOrWhiteSpace(cart.AttributesXml))
                    continue;

                string formattedAttribute = _productAttributeFormatter
                    .FormatAttributes(cart.Product, cart.AttributesXml);
                string[] attributes = formattedAttribute.Split("<br />");

                string colorAttributesString = attributes
                    .FirstOrDefault(x => x.Contains(ProductPictureModifierDefault.ProductAttributeName));

                string customUploadAttribute = attributes.FirstOrDefault(x =>
                    x.Contains($"{ProductPictureModifierDefault.ProductAttributeNameForLogoUpload}: <a href="));

                if (string.IsNullOrWhiteSpace(colorAttributesString) || colorAttributesString.Split(":").Length != 2)
                    continue;

                (int, string) mergedPicture = (0, "");
                if (!string.IsNullOrEmpty(customUploadAttribute))
                {
                    //Get the download
                    Download download = _productPictureModifierService.GetDownloadFromAttributeXml(customUploadAttribute);
                   
                    //Get position setting
                    var logoPosition =_logoPositionService.GetByProductId(cart.ProductId);

                    //Get the picture with custom logo
                    mergedPicture = _customLogoService.MergeProductPictureWithLogo(
                        download.DownloadBinary,
                        download.Id,
                        cart.ProductId,
                        ProductPictureModifierUploadType.Custom,
                        logoPosition.Size,
                        logoPosition.Opacity,
                        logoPosition.XCoordinate,
                        logoPosition.YCoordinate,
                        75
                    );
                }

                cartPictureModifierModel.Add(new ProductPictureModifierModel
                {
                    ColorCode = colorAttributesString.Split(":").Last().Trim(),
                    EntityId = cart.Id,
                    CustomImagePath = mergedPicture.Item2
                });
            }

            return View("~/Plugins/Widgets.ProductPictureModifier/Views/PublicCartItemsColor.cshtml", cartPictureModifierModel);
        }
    }
}
