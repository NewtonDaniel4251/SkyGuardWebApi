using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SkyGuard.Core.Services;

namespace SkyGuard.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;

        public FileStorageService(IHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return fileName;
        }

        public async Task<byte[]?> GetFile(string fileName)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "uploads", fileName);
            return await System.IO.File.ReadAllBytesAsync(filePath);
        }

        public Task DeleteFile(string fileName)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "uploads", fileName);
            System.IO.File.Delete(filePath);
            return Task.CompletedTask;
        }
    }
}
