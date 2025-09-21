using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FileShare.Services
{
    public class BlobStorage : IStorage
    {
        private readonly BlobContainerClient _container;
        public BlobStorage(IConfiguration cfg)
        {
            var cs = cfg["Storage:Blob:ConnectionString"]!;
            var name = cfg["Storage:Blob:Container"]!;
            _container = new BlobContainerClient(cs, name);
            _container.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<string> SaveAsync(string key, Stream content, string contentType, CancellationToken ct)
        {
            var blob = _container.GetBlobClient(key);
            await blob.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            }, ct);
            return key;
        }

        public Task<Stream> OpenReadAsync(string key, CancellationToken ct)
            => _container.GetBlobClient(key).OpenReadAsync(cancellationToken: ct);

        public Task DeleteAsync(string key, CancellationToken ct)
            => _container.GetBlobClient(key).DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
    }
}
