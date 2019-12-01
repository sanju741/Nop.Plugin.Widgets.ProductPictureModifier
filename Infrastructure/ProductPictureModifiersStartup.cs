using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.ProductPictureModifier.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.ViewEngines;
using Nop.Web.Framework.Infrastructure.Extensions;


namespace Nop.Plugin.Widgets.ProductPictureModifier.Infrastructure
{
    /// <summary>
    /// Registers newly created custom view engine
    /// </summary>
    public class ProductPictureModifiersStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add object context
            services.AddDbContext<ProductPictureModifierObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ProductPictureModifiersViewEngine());
            });
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        // needs to override the default view of Nop. Hence, the higher priority
        public int Order => 5;
    }
}