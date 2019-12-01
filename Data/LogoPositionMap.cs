using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Widgets.ProductPictureModifier.Domain;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Data
{
    public  class LogoPositionMap: NopEntityTypeConfiguration<LogoPosition>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<LogoPosition> builder)
        {
            builder.ToTable(nameof(LogoPosition));
            builder.HasKey(record => record.Id);

            builder.Property(record => record.XCoordinate).HasColumnType("decimal(18, 4)");
            builder.Property(record => record.YCoordinate).HasColumnType("decimal(18, 4)");
            builder.Property(record => record.Opacity).HasColumnType("decimal(18, 4)");
            builder.Property(record => record.Size).HasColumnType("decimal(18, 4)");
        }

        #endregion

    }
}
