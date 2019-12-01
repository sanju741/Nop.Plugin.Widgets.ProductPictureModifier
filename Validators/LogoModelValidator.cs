using FluentValidation;
using Nop.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Validators
{
    public class LogoModelValidator : BaseNopValidator<LogoModel>
   {
       public LogoModelValidator(ILocalizationService localizationService, IDbContext dbContext)
       {
           RuleFor(x => x.ProductId)
               .NotEmpty().WithMessage(localizationService.GetResource("Admin.Widgets.ProductPictureModifier.Fields.Product.Required"));

           RuleFor(x => x.Opacity)
               .InclusiveBetween(0,1).WithMessage(localizationService.GetResource("Admin.Widgets.ProductPictureModifier.Fields.Opacity.Invalid"));

           RuleFor(x => x.Size)
               .InclusiveBetween(0, 100).WithMessage(localizationService.GetResource("Admin.Widgets.ProductPictureModifier.Fields.Size.Invalid"));
        }
   }
}
