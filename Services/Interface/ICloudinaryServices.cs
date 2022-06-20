using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optima.Services.Interface
{
    public interface ICloudinaryServices
    {
        Task<(string, bool, string)> UploadImage(IFormFile filePath);
        Task<(List<string>, bool, string)> UploadVideo(List<IFormFile> filePaths);
        Task DeleteVideo(string filePath);
        Task<(List<string>, bool, string)> UploadImages(List<IFormFile> filePaths);
        Task DeleteImage(string filePath);
    }
}
