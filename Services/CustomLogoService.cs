using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Media;
using Nop.Services.Seo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Threading;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Services
{

    /// <summary>
    /// Service to combine logo and product's image to form a new image
    /// </summary>
    public class CustomLogoService : PictureService, IDisposable, ICustomLogoService
    {
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IDownloadService _downloadService;
        private readonly IProductPictureModifierService _productPictureModifierService;

        public CustomLogoService(
            IRepository<Picture> pictureRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IWebHelper webHelper,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            MediaSettings mediaSettings,
            IDataProvider dataProvider,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<PictureBinary> pictureBinaryRepository,
            IUrlRecordService urlRecordService,
            IProductService productService,
            IStoreContext storeContext,
            IDownloadService downloadService,
            IProductPictureModifierService productPictureModifierService)
            : base(dataProvider,
                dbContext,
                eventPublisher,
                fileProvider,
                productAttributeParser,
                pictureRepository,
                pictureBinaryRepository,
                productPictureRepository,
                settingService,
                urlRecordService,
                webHelper,
                mediaSettings)
        {
            _storeContext = storeContext;
            _downloadService = downloadService;
            _productPictureModifierService = productPictureModifierService;
        }

        ///<inheritdoc cref="ICustomLogoService.MergeProductPictureWithLogo"/>
        public (int, string) MergeProductPictureWithLogo(
                  byte[] logoBinary,
                  int logoId,
                  int productId,
                  string uploadType,
                  decimal size,
                  decimal opacity,
                  decimal xCoordinate,
                  decimal yCoordinate,
                  int targetSize = 0,
                  string storeLocation = null,
                  bool overrideThumb = false)
        {
            //Get product picture
            ProductPicture productPicture = _productPictureModifierService.GetFirstProductPicture(productId)
                                            ?? throw new ArgumentException("Product does not have any images");

            Picture productImage = productPicture.Picture;

            //Load pictures binary
            byte[] productPictureBinary = LoadPictureBinary(productPicture.Picture);

            //Validate picture binary
            if (logoBinary == null || logoBinary.Length == 0)
                throw new NullReferenceException("Logo is invalid");
            if (productPictureBinary == null || productPictureBinary.Length == 0)
                throw new NullReferenceException("Product Picture is invalid");

            //Get thumb of the image requested
            int storeId = _storeContext.CurrentStore.Id;
            string lastPart = GetFileExtensionFromMimeType(productImage.MimeType);
            string thumbFileName;
            if (storeId == 1)
            {
                if (targetSize == 0)
                {
                    thumbFileName = !string.IsNullOrEmpty(productImage.SeoFilename)
                        ? $"{productImage.Id:0000000}_{uploadType}_{logoId}_{productImage.SeoFilename}.{lastPart}"
                        : $"{productImage.Id:0000000}_{uploadType}_{logoId}_.{lastPart}";
                }
                else
                {
                    thumbFileName = !string.IsNullOrEmpty(productImage.SeoFilename)
                        ? $"{productImage.Id:0000000}_{uploadType}_{logoId}_{productImage.SeoFilename}_{targetSize}.{lastPart}"
                        : $"{productImage.Id:0000000}_{uploadType}_{logoId}_{targetSize}.{lastPart}";
                }
            }
            else
            {
                if (targetSize == 0)
                {
                    thumbFileName = !String.IsNullOrEmpty(productImage.SeoFilename)
                        ? $"{productImage.Id:0000000}_{uploadType}_{logoId}_{productImage.SeoFilename}_{storeId}.{lastPart}"
                        : $"{productImage.Id:0000000}_{uploadType}_{logoId}_{storeId}.{lastPart}";
                }
                else
                {
                    thumbFileName = !String.IsNullOrEmpty(productImage.SeoFilename)
                        ? $"{productImage.Id:0000000}_{uploadType}_{logoId}_{productImage.SeoFilename}_{targetSize}_{storeId}.{lastPart}"
                        : $"{productImage.Id:0000000}_{uploadType}_{logoId}_{targetSize}_{storeId}.{lastPart}";
                }
            }

            string thumbFilePath = GetThumbLocalPath(thumbFileName);

            //the named mutex helps to avoid creating the same files in different threads,
            //and does not decrease performance significantly, because the code is blocked only for the specific file.
            var mergedImage = new Picture();
            using (var mutex = new Mutex(false, thumbFileName))
            {
                if (!GeneratedThumbExists(thumbFilePath, thumbFileName) || overrideThumb)
                {
                    mutex.WaitOne();

                    //check, if the file was created, while we were waiting for the release of the mutex.
                    if (!GeneratedThumbExists(thumbFilePath, thumbFileName)|| overrideThumb )
                    {
                        byte[] pictureBinaryResized;

                        //resizing required
                        if (targetSize != 0)
                        {
                            using (var stream = new MemoryStream(logoBinary))
                            {
                                //resizing required
                                using (var originImage = Image.Load(logoBinary, out var imageFormat))
                                {
                                    using (var originImageForProduct =
                                        Image.Load(productPictureBinary, out var productImageFormat))
                                    {
                                        Image<Rgba32> image = CombineImageWithLogo(
                                            originImageForProduct, originImage, size, opacity, xCoordinate, yCoordinate);

                                        originImageForProduct.Mutate(imageProcess => imageProcess.Resize(new ResizeOptions
                                        {
                                            Mode = ResizeMode.Max,
                                            Size = CalculateDimensions(originImageForProduct.Size(), targetSize)
                                        }));

                                        pictureBinaryResized = EncodeImage(image, imageFormat);
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (var originImage = Image.Load(logoBinary, out var imageFormat))
                            {
                                using (var originImageForProduct = Image.Load(productPictureBinary, out var productImageFormat))
                                {
                                    Image<Rgba32> image = CombineImageWithLogo(
                                        originImageForProduct, originImage, size, opacity, xCoordinate, yCoordinate);
                                    pictureBinaryResized = EncodeImage(image, imageFormat);
                                }
                            }
                        }

                        SaveThumb(thumbFilePath, thumbFileName, productImage.MimeType, pictureBinaryResized);

                        //We need the new image saved in picture table for pre defined logo created by admin
                        if (uploadType == ProductPictureModifierUploadType.Predefined)
                        {
                            mergedImage = InsertPicture(
                                pictureBinaryResized,
                                productImage.MimeType,
                                productImage.SeoFilename,
                                ProductPictureModifierDefault.MergedPictureAlt,
                                ProductPictureModifierDefault.MergedPictureTitle);
                        }
                    }

                    mutex.ReleaseMutex();
                }
            }
            return (mergedImage.Id, GetThumbUrl(thumbFileName, storeLocation));
        }

        private Image<Rgba32> CombineImageWithLogo(Image<Rgba32> sourceImage, Image<Rgba32> logoImage, decimal size,
            decimal opacity,
             decimal xCoordinate = 0, decimal yCoordinate = 0)
        {
            //Adjust logo image size with respect to Source Image
            var sourceImageSize = new Size((int)(sourceImage.Width), (int)(sourceImage.Height));
            Size adjustedLogoSize = AdjustLogoSize(sourceImageSize, size);
            if (adjustedLogoSize.Width == 0 || adjustedLogoSize.Height == 0)
                return sourceImage;
            logoImage.Mutate(w => w.Resize(adjustedLogoSize));

            //Adjust logo image's position with respect to source image
            Point logoPosition = CalculateLogoPosition(sourceImage.Size(), adjustedLogoSize, xCoordinate, yCoordinate);
            sourceImage.Mutate(d => d.DrawImage(logoImage, (float)opacity, logoPosition));

            return sourceImage;
        }

        private static Size AdjustLogoSize(Size sourceImageSize, decimal logoSizeInPercent)
        {
            decimal logoWidth = logoSizeInPercent / 100 * sourceImageSize.Width;
            decimal logoHeight = logoSizeInPercent / 100 * sourceImageSize.Height;
            return new Size((int)logoWidth, (int)logoHeight);
        }

        private static Point CalculateLogoPosition(Size imageSize, Size logoSize, decimal xCoordinate = 0, decimal yCoordinate = 0)
        {
            var position = new Point
            {
                X = (int)xCoordinate - (logoSize.Width / 2),
                Y = (int)yCoordinate - (logoSize.Height / 2)
            };

            return position;
        }

        #region IDisposable

        private void ReleaseUnmanagedResources()
        {

        }

        public void Dispose()
        {
        }

        ~CustomLogoService()
        {
        }

        #endregion
    }
}
