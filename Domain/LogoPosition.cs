using Nop.Core;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Domain
{
    /// <summary>
    /// Logo's setting for custom upload
    /// </summary>
    public class LogoPosition : BaseEntity
    {
        public int ProductId { get; set; }
        public decimal XCoordinate { get; set; }
        public decimal YCoordinate { get; set; }
        public decimal Opacity { get; set; }
        public decimal Size { get; set; }
        public string ColorCode { get; set; }
    }
}
