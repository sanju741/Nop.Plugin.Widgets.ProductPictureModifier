using FluentValidation.Attributes;
using Nop.Plugin.Widgets.ProductPictureModifier.Validators;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Models
{
    [Validator(typeof(LogoModelValidator))]
    public class LogoModel : BaseNopModel
    {
        public int AttributeValueId { get; set; }
        public int ProductId { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Widgets.ProductPictureModifiers.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifiers.Fields.Opacity")]
        public decimal Opacity { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifiers.Fields.SizeInPercent")]
        public decimal Size { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifiers.Fields.Position.XCoordinate")]
        public decimal XCoordinate { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifiers.Fields.Position.YCoordinate")]
        public decimal YCoordinate { get; set; }

        [NopResourceDisplayName("Widgets.ProductPictureModifiers.Fields.Mark.SettingAs.Default")]
        public bool MarkAsDefault { get; set; }
    }
}
