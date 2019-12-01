using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.Domain;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Web.Factories;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Infrastructure
{
    /// <summary>
    /// Registers new service of the plugin
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //object context
            builder.RegisterPluginDataContext<ProductPictureModifierObjectContext>("nop_object_context_product_picture_modifier");
            builder.RegisterType<EfRepository<LogoPosition>>().As<IRepository<LogoPosition>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_product_picture_modifier"))
                .InstancePerLifetimeScope();

            //services
            builder.RegisterType<LogoPositionService>().As<ILogoPositionService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomLogoService>().As<ICustomLogoService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductPictureModifierService>().As<IProductPictureModifierService>()
                .InstancePerLifetimeScope();

            //override ProductModelFactory
            builder.RegisterType<ProductPictureModifierProductModelFactory>().As<IProductModelFactory>().InstancePerLifetimeScope();
        }

        //high priority for overriding
        public int Order => 5;
    }
}
