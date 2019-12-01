using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Components
{
    /// <summary>
    ///Enables Product Detail Page color selector wheel
    /// </summary>
    public class WidgetsProductPictureModifier : NopViewComponent
    {
        private readonly IProductPictureModifierService _productPictureModifierService;
        private readonly IProductAttributeService _productAttributeService;

        public WidgetsProductPictureModifier(
            IProductPictureModifierService pePictureModifierService,
            IProductAttributeService productAttributeService)
        {
            _productPictureModifierService = pePictureModifierService;
            _productAttributeService = productAttributeService;

        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!(additionalData is ProductDetailsModel))
                return Content("");

            var product = (ProductDetailsModel)additionalData;

            //Get product attribute and mapping
            ProductAttribute productAttribute = _productPictureModifierService.
                GetProductAttributeByName(ProductPictureModifierDefault.ProductAttributeName).First();
            IList<ProductAttributeMapping> productAttributeMappings = _productAttributeService.
                GetProductAttributeMappingsByProductId(product.Id);

            ProductAttributeMapping productAttributeForCustomColorSelector = productAttributeMappings
                .FirstOrDefault(x => x.ProductAttributeId == productAttribute.Id);

            //the product does not support custom color selector
            if (productAttributeForCustomColorSelector == null)
                return Content("");

            var model = new ProductPictureModifierModel()
            {
                ColorCode = string.IsNullOrWhiteSpace(productAttributeForCustomColorSelector.DefaultValue) ?
                    ProductPictureModifierDefault.DefaultColorCode : productAttributeForCustomColorSelector.DefaultValue
            };
            return View("~/Plugins/Widgets.ProductPictureModifier/Views/PublicProductDetailColorSelector.cshtml", model);
        }
    }
}
