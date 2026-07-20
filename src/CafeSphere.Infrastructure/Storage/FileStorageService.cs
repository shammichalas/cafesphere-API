using CafeSphere.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CafeSphere.Infrastructure.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "uploads", CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var relativePath = Path.Combine(folder, uniqueFileName);
        
        _logger.LogInformation("Uploaded file {FileName} to path {Path}", fileName, relativePath);
        
        await Task.CompletedTask;
        return $"/media/{folder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleted file at {FileUrl}", fileUrl);
        return Task.CompletedTask;
    }
}
