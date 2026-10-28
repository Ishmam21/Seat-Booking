using DAL.Storage;   // ADDED: BLL is allowed to see DAL

namespace BLL.Services;   // CHANGED: use whatever namespace your other services use

public class ImageService
{
    private readonly IImageStorage _storage;   // the DAL class that talks to Azure

    public ImageService(IImageStorage storage) => _storage = storage;

    // ADDED: pass-through for now; later, business rules (size, type checks) can live here
    public Task<string> UploadAsync(Stream content, string contentType, string folder, string extension)
        => _storage.UploadAsync(content, contentType, folder, extension);

    public Task DeleteAsync(string? url) => _storage.DeleteAsync(url);
}