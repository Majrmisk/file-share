using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FileShare.Model
{
    [Index(nameof(Token), IsUnique = true)]
    public class File
    {
        public Guid Id { get; set; }
        public string OwnerId { get; set; } = default!;
        public string OriginalName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public string StoragePath { get; set; } = default!;
        public long SizeBytes { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(64)]
        public string? Token { get; set; }
    }
}
