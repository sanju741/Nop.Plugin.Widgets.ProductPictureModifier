using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.ProductPictureModifier.Models;
using Nop.Plugin.Widgets.ProductPictureModifier.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;
using System;
using System.Linq;

namespace Nop.Plugin.Widgets.ProductPictureModifier.Controllers
{
    /// <summary>
    /// Overrides UploadFileProductAttribute method of ShoppingCartController
    /// The method needs to be overriden to enable the preview of new product image with the custom logo uploaded by user in product detail page
    /// </summary>
    public class CustomLogoUploadController : BasePluginController
    {
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICustomLogoService _customLogoService;
        private readonly MediaSettings _mediaSettings;
        private readonly ILogoPositionService _logoPositionService;

        public CustomLogoUploadController(
            IDownloadService downloadService,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            IProductAttributeService productAttributeService,
            ICustomLogoService customLogoService,
            MediaSettings mediaSettings,
            ILogoPositionService logoPositionService)
        {
            _downloadService = downloadService;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _productAttributeService = productAttributeService;
            _customLogoService = customLogoService;
            _mediaSettings = mediaSettings;
            _logoPositionService = logoPositionService;
        }

        /// <summary>
        /// Method to Override Nop's default implementation of UploadFileProductAttribute from Nop.Web's ShoppingCartController
        /// </summary>
        /// <param name="attributeId">attributeId identifier</param>
        /// <returns>Json result containing path of newly created image</returns>
        [HttpPost]
        public IActionResult UploadFileProductAttribute(int attributeId)
        {
            var attribute = _productAttributeService.GetProductAttributeMappingById(attributeId);
            if (attribute == null || attribute.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty
                });
            }

            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty
                });
            }

            var fileBinary = _downloadService.GetDownloadBits(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (attribute.ValidationFileMaximumSize.HasValue)
            {
                //compare in bytes
                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                if (fileBinary.Length > maxFileSizeBytes)
                {
                    //when returning JSON the mime-type must be set to text/plain
                    //otherwise some browsers will pop-up a "Save As" dialog.
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), attribute.ValidationFileMaximumSize.Value),
                        downloadGuid = Guid.Empty
                    });
                }
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            _downloadService.InsertDownload(download);

            //generate new image merging the uploaded custom logo and product picture
            //newly added features for the override
            var logoPosition = _logoPositionService.GetByProductId(attribute.ProductId);

            (int, string) imagePath = _customLogoService.MergeProductPictureWithLogo(
                download.DownloadBinary,
                download.Id,
                attribute.ProductId,
                ProductPictureModifierUploadType.Custom,
               logoPosition.Size,
                logoPosition.Opacity,
                logoPosition.XCoordinate,
                logoPosition.YCoordinate,
                _mediaSettings.ProductDetailsPictureSize
            );
            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid,
                imagePath = imagePath.Item2, //added property for the override 
            });
        }
    }
}
