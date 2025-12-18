using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bibliotheque.Api.Services;

public interface IImageService
{
    Task<string> SaveImageAsync(int bookId, IFormFile file);
    Task<(Stream Stream, string ContentType)> GetImageAsync(string fileName);
    Task<bool> DeleteImageAsync(string fileName);
}

public class ImageService : IImageService
{
    private readonly string _imagesFolder;

    public ImageService(IWebHostEnvironment env)
    {
        _imagesFolder = Path.Combine(env.ContentRootPath, "wwwroot", "images", "books");
        Directory.CreateDirectory(_imagesFolder);
    }

    public async Task<string> SaveImageAsync(int bookId, IFormFile file)
    {
        if (file == null || file.Length == 0) 
            throw new ArgumentException("Fichier invalide", nameof(file));
        
        var ext = Path.GetExtension(file.FileName);
        var safeExt = string.IsNullOrWhiteSpace(ext) ? ".jpg" : ext.ToLowerInvariant();
        var fileName = $"book_{bookId}_{Guid.NewGuid()}{safeExt}";
        var path = Path.Combine(_imagesFolder, fileName);

        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream);

        return fileName;
    }

    public Task<(Stream Stream, string ContentType)> GetImageAsync(string fileName)
    {
        var path = Path.Combine(_imagesFolder, fileName);
        if (!File.Exists(path)) 
            throw new FileNotFoundException("Image introuvable", path);

        var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = GetMimeType(path);

        return Task.FromResult<(Stream Stream, string ContentType)>((fs, contentType));
    }

    public Task<bool> DeleteImageAsync(string fileName)
    {
        var path = Path.Combine(_imagesFolder, fileName);
        if (!File.Exists(path)) 
            return Task.FromResult(false);

        try
        {
            File.Delete(path);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string GetMimeType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}