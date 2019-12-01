using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Infrastructure
{
    /// <summary>
    /// Overrides the UploadFileProductAttribute route of product detail page
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //override upload file method from product detail page
            routeBuilder.MapLocalizedRoute("OverrideUploadFileProductAttribute", "uploadfileproductattribute/{attributeId:min(0)}",
                new { controller = "CustomLogoUpload", action = "UploadFileProductAttribute" });
        }

        // needs to override the default route of Nop. Hence, the higher priority
        public int Priority => 5;
    }
}
