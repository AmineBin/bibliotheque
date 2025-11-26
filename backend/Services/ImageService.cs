using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class ImageService
    {
        private readonly string _imagesFolder;

        public ImageService(string imagesFolder)
        {
            _imagesFolder = imagesFolder ?? throw new ArgumentNullException(nameof(imagesFolder));
            Directory.CreateDirectory(_imagesFolder);
        }

        public async Task<string> SaveToDiskAsync(Guid livreId, IFormFile file)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("Fichier invalide", nameof(file));
            var ext = Path.GetExtension(file.FileName);
            var safeExt = string.IsNullOrWhiteSpace(ext) ? ".bin" : ext.ToLowerInvariant();
            var fileName = $"{livreId}{safeExt}";
            var path = Path.Combine(_imagesFolder, fileName);

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);

            return fileName;
        }

        public Task<(Stream Stream, string ContentType)> GetStreamAsync(string fileName)
        {
            var path = Path.Combine(_imagesFolder, fileName);
            if (!File.Exists(path)) throw new FileNotFoundException("Image introuvable", path);

            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var contentType = GetMimeType(path);

            return Task.FromResult<(Stream Stream, string ContentType)>((fs, contentType));
        }

        private string GetMimeType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}