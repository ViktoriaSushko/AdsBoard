
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdsBoard.Models;
using AdsBoard.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AdvertisementsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 МБ
    private const string UploadFolder = "images/ads";
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public AdvertisementsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: ADVERTISEMENTS
    public async Task<IActionResult> Index()    
    {
        var ads = await _context.Advertisements.Include(a => a.Category).OrderByDescending(a => a.CreateAt).ToListAsync();
        return View(ads);
    }

    // GET: ADVERTISEMENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var advertisement = await _context.Advertisements.Include(a => a.Category).FirstOrDefaultAsync(m => m.Id == id);
        if (advertisement == null)
        {
            return NotFound();
        }

        return View(advertisement);
    }

    // GET: ADVERTISEMENTS/Create
    public IActionResult Create()
    {
        PopulateCategories();
        return View(new Advertisement());
    }

    private void PopulateCategories(int? selectedId=null)
    {
        ViewBag.CategoryId = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name", selectedId);
    }

    // POST: ADVERTISEMENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Price,City,Phone,CategoryId,IsActive")] Advertisement advertisement,
        IFormFile? imageFile)
    {
        // Эти поля заполняет сервер, а не форма
        advertisement.UserId = CurrentUserId;
        advertisement.CreateAt = DateTime.UtcNow;
        ModelState.Remove(nameof(Advertisement.UserId));

        if (imageFile is { Length: > 0 })
        {
            var error = ValidateImage(imageFile);
            if (error != null) ModelState.AddModelError("imageFile", error);
        }

        if (ModelState.IsValid)
        {
            if (imageFile is { Length: > 0 })
                advertisement.ImageUrl = await SaveImageAsync(imageFile);

            _context.Add(advertisement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        PopulateCategories(advertisement.CategoryId);
        return View(advertisement);
    }



    // GET: ADVERTISEMENTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var advertisement = await _context.Advertisements.FindAsync(id);
        if (advertisement == null)
        {
            return NotFound();
        }
        if (advertisement.UserId != CurrentUserId) return Forbid();
        PopulateCategories(advertisement.CategoryId);
        return View(advertisement);
    }

    // POST: ADVERTISEMENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Description,Price,City,Phone,IsActive,CategoryId")] Advertisement advertisement, IFormFile? imageFile)
    {
        if (id != advertisement.Id)
        {
            return NotFound();
        }

        var existing = await _context.Advertisements.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }
        if (existing.UserId != CurrentUserId) return Forbid();
        ModelState.Remove(nameof(Advertisement.UserId));
        if (imageFile is { Length: > 0 })
        {
            var error = ValidateImage(imageFile);
            if (error != null) ModelState.AddModelError("imageFile", error);
        }

        if (ModelState.IsValid)
        {
            existing.Title = advertisement.Title;
            existing.Description = advertisement.Description;
            existing.Price = advertisement.Price;
            existing.City = advertisement.City;
            existing.Phone = advertisement.Phone;
            existing.IsActive = advertisement.IsActive;
            existing.CategoryId = advertisement.CategoryId;
            if(imageFile is { Length: > 0 })
            {
                DeleteImage(existing.ImageUrl);
                existing.ImageUrl = await SaveImageAsync(imageFile);
            }
               await _context.SaveChangesAsync();
            
           
            return RedirectToAction(nameof(Index));
        }
        advertisement.ImageUrl = existing.ImageUrl;
        PopulateCategories(advertisement.CategoryId);
        return View(advertisement);
    }


    // GET: ADVERTISEMENTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var advertisement = await _context.Advertisements.Include(a => a.Category).FirstOrDefaultAsync(m => m.Id == id);
        if (advertisement == null)
        {
            return NotFound();
        }
        if (advertisement.UserId != CurrentUserId) return Forbid();
        return View(advertisement);
    }

    // POST: ADVERTISEMENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var advertisement = await _context.Advertisements.FindAsync(id);
        if (advertisement != null)
        {
            if (advertisement.UserId != CurrentUserId) return Forbid();
            DeleteImage(advertisement.ImageUrl);
            _context.Advertisements.Remove(advertisement);
            await _context.SaveChangesAsync();
        }
       
        return RedirectToAction(nameof(Index));
    }
    private static string? ValidateImage(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return "Дозволені формати: jpg, jpeg, png, gif, webp";
        if (file.Length > MaxFileSize)
            return "Розмір файлу не повинен перевищувати 5 МБ";
        return null;
    }
    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var folder = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{UploadFolder}/{fileName}";
    }
    private void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith($"/{UploadFolder}/"))
            return;

        var fullPath = Path.Combine(_env.WebRootPath,
            imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }

    private bool AdvertisementExists(int? id)
    {
        return _context.Advertisements.Any(e => e.Id == id);
    }
}
