using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DAL.Storage;

public interface IImageStorage
{
    Task<string> UploadAsync(Stream content, string contentType, string folder, string extension);
    Task DeleteAsync(string? url);
}

public class BlobImageStorage : IImageStorage
{
    private readonly BlobContainerClient _container;

    public BlobImageStorage(string connectionString, string containerName)
    {
        _container = new BlobContainerClient(connectionString, containerName);
        
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string folder, string extension)
    {
        var blob = _container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
        // CHANGED: uploads the stream the caller gives us (no OpenReadStream here)
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string? url)
    {
        var prefix = _container.Uri.ToString().TrimEnd('/') + "/";
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(prefix, StringComparison.Ordinal)) return;
        await _container.DeleteBlobIfExistsAsync(url[prefix.Length..]);
    }


}