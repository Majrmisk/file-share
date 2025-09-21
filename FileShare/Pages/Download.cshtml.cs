using Azure;
using FileShare.Data;
using FileShare.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FileShare.Pages
{
    public class DownloadModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IStorage _storage;

        public DownloadModel(ApplicationDbContext db, IStorage storage)
        {
            _db = db;
            _storage = storage;
        }

        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        public string? Error { get; private set; }
        public string? FileName { get; private set; }
        public long FileSize { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                Error = "Link is not valid.";
                return Page();
            }

            var file = await _db.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Token == Token);

            if (file == null)
            {
                Error = "Link is not valid.";
                return Page();
            }

            FileName = file.OriginalName;
            FileSize = file.SizeBytes;
            return Page();
        }

        public async Task<IActionResult> OnPostDownloadAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                Error = "Link is not valid.";
                return RedirectToPage();
            }

            var file = await _db.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Token == Token);

            if (file == null)
            {
                Error = "Link is not valid.";
                return RedirectToPage();
            }

            try
            {
                var stream = await _storage.OpenReadAsync(file.StoragePath, HttpContext.RequestAborted);
                return File(stream, file.ContentType, file.OriginalName);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Error = "The file could not be found.";
                return Page();
            }
        }
    }
}
