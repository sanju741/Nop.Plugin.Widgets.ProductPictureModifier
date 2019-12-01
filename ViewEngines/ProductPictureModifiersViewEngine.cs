using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.ProductPictureModifier.ViewEngines
{
    /// <summary>
    /// View Engine to override _ProductAttributes.cshtml from Nop's default implementation
    /// </summary>
    public class ProductPictureModifiersViewEngine : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            //nothing to do here.
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            //replace view for this particular type only
            if (context.AreaName == null && context.ViewName == "_ProductAttributes")
            {
                viewLocations = new string[]
                {
                    "/Plugins/Widgets.ProductPictureModifier/Views/_ProductAttributes.cshtml"
                }.Concat(viewLocations);

            }
            if (context.AreaName == null && context.ViewName == "_ProductBox")
            {
                viewLocations = new string[]
                {
                    "/Plugins/Widgets.ProductPictureModifier/Views/_ProductBox.cshtml"
                }.Concat(viewLocations);

            }

            return viewLocations;
        }
    }
}