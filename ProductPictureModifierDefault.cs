using Nop.Plugin.Widgets.ProductPictureModifier.Models;

namespace Nop.Plugin.Widgets.ProductPictureModifier
{
    public static class ProductPictureModifierDefault
    {
        public static string ProductAttributeName => "Custom Color";
        public static string ProductAttributeNameForLogo => "Custom Logo";
        public static string ProductAttributeNameForLogoUpload => "Custom Logo Upload";
        public static string ProductAttributeDescription => "<p>Color selection</p>";
        public static string ProductAttributeDescriptionForLogo => "<p></p>";
        public static string ProductAttributeDescriptionForLogoUpload => "<p></p>";
        public static string DefaultColorCode => "#ffffff";
        public static decimal LogoOpacity => 1;
        public static decimal LogoSizeInPercent => 20;
        public static string UploadIconPictureIdSetting => "Widgets.ProductPictureModifier.UploadIcon.PictureId";
        public static int MergedPictureDisplayOrder => int.MaxValue - 1;
        public static string MergedPictureAlt =>"CustomLogo_PPM";
        public static string MergedPictureTitle =>"CustomLogo_PPM";
    }
}
