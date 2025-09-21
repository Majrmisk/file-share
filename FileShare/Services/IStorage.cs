namespace FileShare.Services
{
    public interface IStorage
    {
        Task<string> SaveAsync(string key, Stream content, string contentType, CancellationToken ct);
        Task<Stream> OpenReadAsync(string key, CancellationToken ct);
        Task DeleteAsync(string key, CancellationToken ct);
    }
}
