using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace BUS_Group4_E_Commerce
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = "profile-photos");
        Task<DeletionResult> DeleteImageAsync(string publicId);
        string GetOptimizedImageUrl(string publicId, int width = 300, int height = 300);
    }
}
