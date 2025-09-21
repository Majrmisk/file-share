using FileShare.Data;
using FileShare.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FileShare.Pages.Files
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IStorage _storage;
        private readonly IConfiguration _config;

        public IndexModel(ApplicationDbContext db, IStorage storage, IConfiguration config)
        {
            _db = db;
            _storage = storage;
            _config = config;
        }

        public List<Model.File> MyFiles { get; private set; } = [];

        [BindProperty]
        public IFormFile? Upload { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            MyFiles = await _db.Files
                .Where(f => f.OwnerId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Upload == null || Upload.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Select a file.");
                await OnGetAsync();
                return Page();
            }

            const long maxBytes = 25 * 1024 * 1024;
            if (Upload.Length > maxBytes)
            {
                ModelState.AddModelError(string.Empty, "File too large.");
                await OnGetAsync();
                return Page();
            }

            var adminEmail = _config["Quotas:AdminEmail"];
            var userLimit = long.TryParse(_config["Quotas:PerUserBytes"], out var limit) ? limit : 0;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (userLimit > 0 && !string.Equals(userEmail, adminEmail))
            {
                var used = await _db.Files
                    .Where(f => f.OwnerId == userId)
                    .SumAsync(f => (long?)f.SizeBytes) ?? 0;

                if (used + Upload.Length > userLimit)
                {
                    ModelState.AddModelError(string.Empty, $"Storage quota exceeded ({userLimit} bytes).");
                    await OnGetAsync();
                    return Page();
                }
            }

            var today = DateTime.UtcNow;
            var key = $"{userId}/{today:yyyy}/{today:MM}/{today:dd}/{Guid.NewGuid():N}";

            var storageKey = await _storage.SaveAsync(
                key,
                Upload!.OpenReadStream(),
                Upload.ContentType,
                HttpContext.RequestAborted
            );

            var item = new Model.File
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                OriginalName = Path.GetFileName(Upload.FileName),
                SizeBytes = Upload.Length,
                ContentType = Upload.ContentType,
                StoragePath = storageKey,
            };

            _db.Files.Add(item);
            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);
            if (file == null) return NotFound();

            try
            {
                if (!string.IsNullOrWhiteSpace(file.StoragePath))
                    await _storage.DeleteAsync(file.StoragePath, HttpContext.RequestAborted);
            }
            catch
            {
            }

            _db.Files.Remove(file);
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateLinkAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);
            if (file == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(file.Token))
            {
                var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
                file.Token = Convert.ToHexString(bytes).ToLowerInvariant();
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRevokeLinkAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var file = await _db.Files.FirstOrDefaultAsync(f => f.Id == id && f.OwnerId == userId);
            if (file == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(file.Token))
            {
                file.Token = null;
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
