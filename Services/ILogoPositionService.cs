using Nop.Plugin.Widgets.ProductPictureModifier.Domain;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Services
{
    /// <summary>
    /// Interface for logo position settings for custom upload
    /// </summary>
    public interface ILogoPositionService
    {
        /// <summary>
        /// Insert logo's setting for custom upload
        /// </summary>
        /// <param name="position">LogoPosition</param>
        void Insert(LogoPosition position);

        /// <summary>
        /// Updated logo's setting for custom upload
        /// </summary>
        /// <param name="position">LogoPosition</param>
        void Update(LogoPosition position);

        /// <summary>
        /// Get Logo's setting by product id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        LogoPosition GetByProductId(int productId);

        /// <summary>
        /// Deletes logo position setting for custom upload by product id
        /// </summary>
        /// <param name="productId"></param>
        void DeleteByProductId(int productId);
    }
}