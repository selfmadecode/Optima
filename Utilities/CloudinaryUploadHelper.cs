using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optima.Utilities
{
    public class CloudinaryUploadHelper
    {
        /// <summary>
        /// Uploads the specified documents
        /// </summary>
        /// <param name="filePaths">The Files</param>
        /// <param name="configuration">The File</param>
        /// <returns>
        /// System.ValueTuple&lt;List&lt;System.String&gt;, System.Boolean, System.String&gt;.
        /// </returns>
        public static async Task<(List<string>, bool, string)> UploadVideo(List<IFormFile> filePaths, IConfiguration configuration)
        {
            var fileUrl = new List<string>();

            if (!filePaths.Any())
            {
                return (null, false, "Success");
            }

            try
            {

                Account account = new Account()
                {
                    Cloud = configuration["Cloudinary:Cloud"],
                    ApiKey = configuration["Cloudinary:ApiKey"],
                    ApiSecret = configuration["Cloudinary:ApiSecret"]
                };

                Cloudinary cloudinary = new Cloudinary(account);
                cloudinary.Api.Secure = true;

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

                    var uploadResult = await cloudinary.UploadAsync(uploadParams);

                    fileUrl.Add(uploadResult.SecureUrl.AbsoluteUri);

                }

                return (fileUrl, false, "Success");

            }
            catch (Exception ex)
            {
                return (fileUrl, true, string.Join(",", ex.Message, "An Error Occcured"));
            }

        }

        /// <summary>
        /// Deletes the specified documents
        /// </summary>
        /// <param name="configuration">The Files</param>
        /// <param name="filePath">The File</param>
        public static void DeleteVideo(IConfiguration configuration, string filePath)
        {
            Account account = new Account()
            {
                Cloud = configuration["Cloudinary:Cloud"],
                ApiKey = configuration["Cloudinary:ApiKey"],
                ApiSecret = configuration["Cloudinary:ApiSecret"]
            };

            Cloudinary cloudinary = new Cloudinary(account);

            var deletionParams = new DeletionParams(filePath)
            {
                ResourceType = ResourceType.Video,
            };

            cloudinary.Destroy(deletionParams);

        }

        /// <summary>
        /// Uploads the specified documents
        /// </summary>
        /// <param name="filePaths">The Files</param>
        /// <param name="configuration">The File</param>
        /// <returns>
        /// System.ValueTuple&lt;List&lt;System.String&gt;, System.Boolean, System.String&gt;.
        /// </returns>
        public static async Task<(List<string>, bool, string)> UploadImages(List<IFormFile> filePaths, IConfiguration configuration)
        {
            var fileUrl = new List<string>();

            if (!filePaths.Any())
            {
                return (null, false, "Success");
            }

            try
            {

                Account account = new Account()
                {
                    Cloud = configuration["Cloudinary:Cloud"],
                    ApiKey = configuration["Cloudinary:ApiKey"],
                    ApiSecret = configuration["Cloudinary:ApiSecret"]
                };

                Cloudinary cloudinary = new Cloudinary(account);
                cloudinary.Api.Secure = true;

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

                    var uploadResult = await cloudinary.UploadAsync(uploadParams);

                    fileUrl.Add(uploadResult.SecureUrl.AbsoluteUri);

                }

                return (fileUrl, false, "Success");

            }
            catch (Exception ex)
            {
                return (fileUrl, true, string.Join(",", ex.Message, "An Error Occcured"));
            }
        }

        /// <summary>
        /// Uploads the specified document
        /// </summary>
        /// <param name="filePath">The File</param>
        /// <param name="configuration">The Configuration</param>
        /// <returns>
        /// System.ValueTuple&lt;List&lt;System.String&gt;, System.Boolean, System.String&gt;.
        /// </returns>
        public static async Task<(string, bool, string)> UploadImage(IFormFile filePath, IConfiguration configuration)
        {
            string fileUrl = default;

            if(filePath is null)
            {
                return (null, false, "Success");
            }

            try
            {

                Account account = new Account()
                {
                    Cloud = configuration["Cloudinary:Cloud"],
                    ApiKey = configuration["Cloudinary:ApiKey"],
                    ApiSecret = configuration["Cloudinary:ApiSecret"]
                };

                Cloudinary cloudinary = new Cloudinary(account);
                cloudinary.Api.Secure = true;

                var stream = filePath.OpenReadStream();

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(filePath.FileName, stream),
                    Folder = "Optima",
                    Invalidate = true,
                    UseFilename = true,
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                fileUrl = uploadResult.SecureUrl.AbsoluteUri;


                return (fileUrl, false, "Success");

            }
            catch (Exception ex)
            {
                return (fileUrl, true, string.Join(",", ex.Message, "An Error Occcured"));
            }

        }

        /// <summary>
        /// Deletes the specified document
        /// </summary>
        /// <param name="configuration">The Configuration</param>
        /// <param name="filePath">The FilePath</param>
        public static void DeleteImage(IConfiguration configuration, string filePath)
        {
            Account account = new Account()
            {
                Cloud = configuration["Cloudinary:Cloud"],
                ApiKey = configuration["Cloudinary:ApiKey"],
                ApiSecret = configuration["Cloudinary:ApiSecret"]
            };

            Cloudinary cloudinary = new Cloudinary(account);

            var deletionParams = new DeletionParams(filePath)
            {
                ResourceType = ResourceType.Image
            };

            cloudinary.Destroy(deletionParams);

        }
                      
    } 
}