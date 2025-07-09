using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BUS_Group4_E_Commerce
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudinaryUrl = configuration["CloudinarySettings:CloudinaryUrl"];
            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                Debug.WriteLine("Cloudinary URL is not configured");
                throw new ArgumentException("Cloudinary URL is not configured");
            }
            Debug.WriteLine("Cloudinary URL success");

            _cloudinary = new Cloudinary(cloudinaryUrl);
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = "profile-photos")
        {
            if (file == null || file.Length == 0)
            {
                Debug.WriteLine("File is required");
                throw new ArgumentException("File is required");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                Debug.WriteLine("Only JPEG, PNG, and GIF files are allowed");
                throw new ArgumentException("Only JPEG, PNG, and GIF files are allowed");
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                Debug.WriteLine("File size cannot exceed 5MB");
                throw new ArgumentException("File size cannot exceed 5MB");
            }

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Quality("auto")
                    .FetchFormat("auto"),
                PublicId = $"{folder}/{Guid.NewGuid()}"
            };
            Debug.WriteLine("upload img success");

            return await _cloudinary.UploadAsync(uploadParams);
        }

        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                throw new ArgumentException("Public ID is required");
            }

            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }

        public string GetOptimizedImageUrl(string publicId, int width = 300, int height = 300)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return "/images/default-avatar.png"; // Default avatar
            }

            return _cloudinary.Api.UrlImgUp
                .Transform(new Transformation()
                    .Width(width)
                    .Height(height)
                    .Crop("fill")
                    .Quality("auto")
                    .FetchFormat("auto"))
                .BuildUrl(publicId);
        }
    }
}
