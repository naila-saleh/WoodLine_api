using BakerGroup.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BakerGroup.BLL.Services.Classes;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        var wwwrootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrEmpty(wwwrootPath))
        {
            // If WebRootPath is null (common in some environments if wwwroot doesn't exist), 
            // we'll default to the content root's wwwroot.
            wwwrootPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var path = Path.Combine(wwwrootPath, "uploads", folderName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(path, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{folderName}/{fileName}";
    }

    public async Task<List<string>> UploadFilesAsync(IFormFileCollection files, string folderName)
    {
        var filePaths = new List<string>();
        foreach (var file in files)
        {
            var filePath = await UploadFileAsync(file, folderName);
            if (!string.IsNullOrEmpty(filePath))
            {
                filePaths.Add(filePath);
            }
        }
        return filePaths;
    }

    public void DeleteFile(string fileName, string folderName)
    {
        var wwwrootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        var path = Path.Combine(wwwrootPath, "uploads", folderName, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
