using Nop.Core.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.Domain;
using System;
using System.Linq;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Services
{
    /// <summary>
    /// Class for logo position settings for custom upload
    /// </summary>
    public class LogoPositionService : ILogoPositionService
    {
        private readonly IRepository<LogoPosition> _logoPositionRepository;

        public LogoPositionService(IRepository<LogoPosition> logoPositionRepository)
        {
            _logoPositionRepository = logoPositionRepository;
        }

        /// <inheritdoc cref="ILogoPositionService.Insert"/>
        public void Insert(LogoPosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            _logoPositionRepository.Insert(position);
        }

        /// <inheritdoc cref="ILogoPositionService.Update"/>
        public void Update(LogoPosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            _logoPositionRepository.Update(position);
        }

        /// <inheritdoc cref="ILogoPositionService.GetByProductId"/>
        public LogoPosition GetByProductId(int productId)
        {
            if (productId < 1)
                throw new InvalidOperationException();

            var query = _logoPositionRepository.Table.FirstOrDefault(x => x.ProductId == productId);
            return query;
        }

        /// <inheritdoc cref="ILogoPositionService.DeleteByProductId"/>
        public void DeleteByProductId(int productId)
        {
            if (productId < 1)
                throw new InvalidOperationException();

            var logos = _logoPositionRepository.Table.Where(x => x.ProductId == productId);
            _logoPositionRepository.Delete(logos);
        }
    }
}
