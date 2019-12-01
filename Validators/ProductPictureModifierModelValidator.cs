using FluentValidation;
using Nop.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Validators
{
    public class ProductPictureModifierModelValidator : BaseNopValidator<ProductPictureModifierModel>
    {
        public ProductPictureModifierModelValidator(ILocalizationService localizationService, IDbContext dbContext)
        {
            RuleFor(x => x.ProductId)
                .NotNull().GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Widgets.ProductPictureModifier.Fields.Product.Required"));
        }
    }
}
