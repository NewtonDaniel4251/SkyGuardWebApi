
using Microsoft.AspNetCore.Http;

namespace SkyGuard.Core.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFile(IFormFile file);
        Task<byte[]> GetFile(string filePath);
        Task DeleteFile(string filePath);
    }
}
