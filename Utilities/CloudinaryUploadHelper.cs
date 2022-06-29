using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Optima.Models.Config;
using Optima.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Services.Implementation
{
    public class CloudinaryUploadHelper : ICloudinaryServices
    {
        private readonly ILog _logger;
        private readonly CloudinaryAccount _cloudinaryAccount;
        private readonly Account _account;
        private readonly Cloudinary _cloudinary;

        public CloudinaryUploadHelper(IOptions<CloudinaryAccount> cloudinaryAccount)
        {
            _logger = LogManager.GetLogger(typeof(CloudinaryUploadHelper));
            _cloudinaryAccount = cloudinaryAccount.Value;
            _cloudinary = new Cloudinary(_account = SetupCloudinary());
        }
        
        public async Task<(List<string>, bool, string)> UploadVideo(List<IFormFile> filePaths)
        {
            var fileUrl = new List<string>();

            if (!filePaths.Any())
            {
                _logger.Error("File path is null");
                return (null, true, "Failed");
            }

            try
            {
                //Cloudinary cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;

                foreach (var filepath in filePaths)
                {       
                    var stream = filepath.OpenReadStream();

                    var uploadParams = new VideoUploadParams()
                    {
                        File = new FileDescription(filepath.FileName, stream),
                        Folder = "Optima",
                        Invalidate = true,
                        UseFilename = true,
                        EagerTransforms = new List<Transformation>
                        {
                           new EagerTransformation().Width(500).Height(400).Crop("pad"),
                        },

                        EagerAsync = true,
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    fileUrl.Add(uploadResult.SecureUrl.AbsoluteUri);
                }

                return (fileUrl, false, "Success");

            }
            catch (Exception ex)
            {
                _logger.Error($"An Error Occured while uploading file to cloudinary -- {ex.Message}");
                return (fileUrl, true, string.Join(",", ex.Message, "An Error Occcured"));
            }

        }        
                
        public async Task DeleteVideo(string filePath)
        {
            var deletionParams = new DeletionParams(filePath)
            {
                ResourceType = ResourceType.Video,
            };

            await _cloudinary.DestroyAsync(deletionParams);
        }

        public async Task<(List<string>, bool, string)> UploadImages(List<IFormFile> filePaths)
        {
            var fileUrl = new List<string>();

            if (!filePaths.Any())
            {
                _logger.Error("File path is null");
                return (null, true, "Failed");
            }

            try
            {
                _cloudinary.Api.Secure = true;

                foreach (var filepath in filePaths)
                {

                    var stream = filepath.OpenReadStream();
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(filepath.FileName, stream),
                        Folder = "RapX",
                        Invalidate = true,
                        UseFilename = true,
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    fileUrl.Add(uploadResult.SecureUrl.AbsoluteUri);

                }

                return (fileUrl, false, "Success");

            }
            catch (Exception ex)
            {
                _logger.Error($"An Error Occured while uploading file to cloudinary -- {ex.Message}");
                return (fileUrl, true, string.Join(",", ex.Message, "An Error Occcured"));
            }
        }
                
        public async Task<(string, bool, string)> UploadImage(IFormFile filePath)
        {
            string fileUrl = default;

            if(filePath is null)
            {
                _logger.Error("File path is null");
                return (null, true, "Failed");
            }

            try
            {
                _cloudinary.Api.Secure = true;

                var stream = filePath.OpenReadStream();

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(filePath.FileName, stream),
                    Folder = "Optima",
                    Invalidate = true,
                    UseFilename = true,
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                fileUrl = uploadResult.SecureUrl.AbsoluteUri;

                return (fileUrl, false, "Success");
            }
            catch (Exception ex)
            {
                _logger.Error($"An Error Occured while uploading file to cloudinary -- {ex.Message}");
                return (fileUrl, true, string.Join(",", ex.Message, "An Error Occcured while uploading file to cloudinary"));
            }
        }
        public async Task DeleteImage(string filePath)
        {
            //Cloudinary cloudinary = new Cloudinary(account);

            var deletionParams = new DeletionParams(filePath)
            {
                ResourceType = ResourceType.Image
            };

            await _cloudinary.DestroyAsync(deletionParams);
        }

        private Account SetupCloudinary()
        {
            return new Account()
            {
                Cloud = _cloudinaryAccount.Cloud,
                ApiKey = _cloudinaryAccount.ApiKey,
                ApiSecret = _cloudinaryAccount.ApiSecret
            };
        }

    } 
}