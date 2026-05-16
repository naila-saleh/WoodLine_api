using Microsoft.AspNetCore.Http;

namespace BakerGroup.BLL.Services.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    Task<List<string>> UploadFilesAsync(IFormFileCollection files, string folderName);
    void DeleteFile(string fileName, string folderName);
}
