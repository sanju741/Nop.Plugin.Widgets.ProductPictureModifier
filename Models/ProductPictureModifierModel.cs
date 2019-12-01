using FluentValidation.Attributes;
using Nop.Plugin.Widgets.ProductPictureModifier.Validators;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Models
{
    /// <summary>
    /// Model class for the plugin
    /// </summary>

    [Validator(typeof(ProductPictureModifierModelValidator))]
    public class ProductPictureModifierModel : BaseNopModel
    {
        public ProductPictureModifierModel()
        {
            LogoSearchModel = new LogoSearchModel();
            LogoModel = new LogoModel();
        }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int EntityId { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifier.Admin.Fields.ColorCode")]
        public string ColorCode { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifier.Admin.Fields.ProductName")]
        public string ProductName { get; set; }
        public string CustomImagePath { get; set; }
        public LogoSearchModel LogoSearchModel { get; set; }
        public LogoModel LogoModel { get; set; }
    }
}
