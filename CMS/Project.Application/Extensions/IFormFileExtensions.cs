using Microsoft.AspNetCore.Http;
using Project.Application.Helpers;

namespace Project.Application.Extensions
{
    public static class IFormFileExtensions
    {
        public static bool IsValidMime(this IFormFile? file, string[] mimeTypes)
        {
            if (file == null || file.Length < 1)
            {
                return false;
            }

            return mimeTypes.Contains(file.ContentType.ToLower());
        }

        public static bool IsValidExtension(this IFormFile? file, string[] extensions)
        {
            if (file == null || file.Length < 1)
            {
                return false;
            }

            return extensions.Contains(Path.GetExtension(file.FileName).ToLower());
        }

        public static bool IsValidImage(this IFormFile? file)
        {
            if (file == null || file.Length < 1)
            {
                return false;
            }
            string mimeType = file.ContentType.ToLower();
            if (mimeType != "image/jpg" &&
                 mimeType != "image/jpeg" &&
                 mimeType != "image/png")
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            return extension == ".jpg" || extension == ".jpeg" || extension == ".png";
        }

        public static bool IsValidVideo(this IFormFile? file)
        {
            if (file == null || file.Length < 1)
            {
                return false;
            }
            string mimeType = file.ContentType.ToLower();
            if (mimeType != "video/mp4")
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            return extension == ".mp4";
        }

        public static bool IsValidVoice(this IFormFile? file)
        {
            if (file == null || file.Length < 1)
            {
                return false;
            }
            string mimeType = file.ContentType.ToLower();
            if (mimeType != "audio/mp3" &&
                 mimeType != "audio/x-m4a")
            {
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            return extension == ".mp3" || extension == ".m4a";
        }
    }
}
