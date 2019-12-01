using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Models
{
    /// <summary>
    /// Model for search option in the listing page
    /// </summary>
    public class ProductPictureModifierSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Widgets.ProductPictureModifier.Admin.Fields.Keywords")]
        public string Keywords { get; set; }
        public int ProductId { get; set; }
    }
}
