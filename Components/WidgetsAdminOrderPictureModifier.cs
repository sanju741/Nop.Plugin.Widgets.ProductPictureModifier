using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Plugin.Widgets.ProductPictureModifier.Domain;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Components
{
    /// <summary>
    /// Admin Order's product picture customization based on the selected attributes
    /// </summary>
    public class WidgetsAdminOrderPictureModifier : NopViewComponent
    {
        private readonly ICustomLogoService _customLogoService;
        private readonly IProductPictureModifierService _productPictureModifierService;
        private readonly ILogoPositionService _logoPositionService;

        public WidgetsAdminOrderPictureModifier(
            ICustomLogoService customLogoService,
            IProductPictureModifierService productPictureModifierService,
            ILogoPositionService logoPositionService)
        {
            _customLogoService = customLogoService;
            _productPictureModifierService = productPictureModifierService;
            _logoPositionService = logoPositionService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            //Validate the request is being invoked from admin order's page
            if (!(additionalData is OrderModel order))
                return View("~/Plugins/Widgets.ProductPictureModifier/Views/PublicCartColorSelector.cshtml",
                    new List<ProductPictureModifierModel>());

            var cartPictureModifierModel = new List<ProductPictureModifierModel>();
            foreach (var orderItem in order.Items)
            {
                string[] attributes = orderItem.AttributeInfo.Split("<br />");

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
                    LogoPosition logoPosition = _logoPositionService.GetByProductId(orderItem.ProductId);

                    //Get the picture with custom logo
                    mergedPicture = _customLogoService.MergeProductPictureWithLogo(
                        download.DownloadBinary,
                        download.Id,
                        orderItem.ProductId,
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
                    EntityId = orderItem.Id,
                    CustomImagePath = mergedPicture.Item2
                });
            }

            return View("~/Plugins/Widgets.ProductPictureModifier/Views/AdminOrderItemsColor.cshtml", cartPictureModifierModel);
        }
    }
}
