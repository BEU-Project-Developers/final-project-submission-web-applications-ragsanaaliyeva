// FurnitureShopProjectRazil.Areas.Admin.Controllers.AboutController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using FurnitureManagementSystem.Models; // Dəyişdirildi
using System.IO;
using System.Threading.Tasks;
using FurnitureManagementSystem.Data; // Dəyişdirildi (DbContext-in olduğu namespace)
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FurnitureManagementSystem.Areas.Admin.Controllers // Dəyişdirildi
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Rol adını öz proyektinizə uyğunlaşdırın
    public class AboutController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context; // AppDbContext adının düzgün olduğundan əmin olun
        private readonly ILogger<AboutController> _logger;

        // Şəkillərin yüklənəcəyi qovluq (wwwroot içində)
        private const string ImageUploadDirectory = "images/profiles"; // Qovluq adını istəyə görə dəyişə bilərsiniz

        public AboutController(AppDbContext context, IWebHostEnvironment env, ILogger<AboutController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var abouts = await _context.Abouts.ToListAsync(); // Model adı dəyişmədiyi üçün eyni qalır
            return View(abouts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (await _context.Abouts.AnyAsync())
            {
                var existingAbout = await _context.Abouts.FirstOrDefaultAsync();
                if (existingAbout != null)
                {
                    TempData["InfoMessage"] = "'Haqqımızda' məlumatı artıq mövcuddur. Yalnız redaktə edə bilərsiniz.";
                    return RedirectToAction(nameof(Update), new { id = existingAbout.Id });
                }
            }
            return View(new About()); // Yeni model obyektini göndəririk
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(About model)
        {
            if (await _context.Abouts.AnyAsync())
            {
                ModelState.AddModelError(string.Empty, "'Haqqımızda' məlumatı artıq yaradılıb. Yalnız mövcud olanı redaktə edə bilərsiniz.");
                // return View(model); // Bu sətirə ehtiyac yoxdur, çünki yuxarıdakı AnyAsync yoxlaması onsuz da xəta verəcək
            }

            if (ModelState.IsValid)
            {
                if (model.Photo != null && model.Photo.Length > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(_env.WebRootPath, ImageUploadDirectory);
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                            _logger.LogInformation("Created directory: {DirectoryPath}", folderPath);
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Photo.CopyToAsync(stream);
                        }
                        model.ImagePath = Path.Combine(ImageUploadDirectory, fileName).Replace("\\", "/");
                        _logger.LogInformation("Image uploaded for new About section: {ImagePath}", model.ImagePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for new About section.");
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        return View(model);
                    }
                }
                else if (string.IsNullOrEmpty(model.ImagePath)) // Əgər şəkil yüklənməyibsə və mövcud şəkil yolu yoxdursa
                {
                    // Default şəkil təyin edə bilərsiniz və ya boş qala bilər
                    // model.ImagePath = "/path/to/default-image.jpg";
                    // Və ya validation xətası:
                    // ModelState.AddModelError("Photo", "Şəkil tələb olunur.");
                    // return View(model);
                    // Qeyd: Modelinizdə ImagePath [Required] deyil, ona görə bu hissə şərtidir.
                    // Əgər şəkil mütləqdirsə, modeldə [Required] edin və ya burada yoxlayın.
                }


                await _context.Abouts.AddAsync(model);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New About section created with ID {Id}", model.Id);
                TempData["SuccessMessage"] = "'Haqqımızda' məlumatı uğurla yaradıldı.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                var firstAbout = await _context.Abouts.FirstOrDefaultAsync();
                if (firstAbout == null)
                {
                    _logger.LogWarning("Update GET: No About section found to update, redirecting to Create.");
                    TempData["InfoMessage"] = "Redaktə ediləcək 'Haqqımızda' məlumatı tapılmadı. Zəhmət olmasa, birini yaradın.";
                    return RedirectToAction(nameof(Create));
                }
                id = firstAbout.Id;
            }

            var modelFromDb = await _context.Abouts.FindAsync(id.Value);
            if (modelFromDb == null)
            {
                _logger.LogWarning("Update GET: About section with ID {Id} not found.", id);
                return NotFound();
            }
            return View(modelFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, About model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            // Model state yoxlanarkən Photo null olsa da, ImagePath mövcud ola bilər.
            // Bu halda ModelState.IsValid false qaytarmamalıdır.
            // Ona görə Photo üçün olan validation-u ayrıca idarə edə bilərik və ya
            // IFormFile üçün Required atributu istifadə etməyə bilərik (NotMapped olduğu üçün).
            // Əgər Photo null gəlirsə və ModelState-də "Photo" üçün xəta varsa, bunu silək.
            if (model.Photo == null && ModelState.ContainsKey("Photo"))
            {
                ModelState.Remove("Photo");
            }


            if (ModelState.IsValid)
            {
                var existingAbout = await _context.Abouts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.Id);
                if (existingAbout == null) return NotFound();

                string? oldImagePath = existingAbout.ImagePath;
                string newImagePath = existingAbout.ImagePath; // Default olaraq köhnəni saxlayırıq

                if (model.Photo != null && model.Photo.Length > 0)
                {
                    try
                    {
                        string folderPath = Path.Combine(_env.WebRootPath, ImageUploadDirectory);
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Photo.CopyToAsync(stream);
                        }
                        newImagePath = Path.Combine(ImageUploadDirectory, fileName).Replace("\\", "/");
                        _logger.LogInformation("Image updated for About section ID {Id}: {ImagePath}", model.Id, newImagePath);

                        // Köhnə şəkli sil (əgər fərqlidirsə və default şəkil deyilsə)
                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != newImagePath && !oldImagePath.Contains("default-about-image.png"))
                        {
                            string fullOldPath = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullOldPath))
                            {
                                System.IO.File.Delete(fullOldPath);
                                _logger.LogInformation("Old image deleted: {OldImagePath}", fullOldPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image for About section ID {Id}.", model.Id);
                        ModelState.AddModelError("Photo", "Şəkil yüklənərkən xəta baş verdi.");
                        model.ImagePath = oldImagePath; // Xəta baş verərsə, köhnə şəkil yolunu view-a qaytar
                        return View(model);
                    }
                }

                // Update existingAbout from DB with values from model
                // Öncə context-dən track olunan entity-ni almalıyıq
                var entityToUpdate = await _context.Abouts.FindAsync(model.Id);
                if (entityToUpdate == null) return NotFound();

                entityToUpdate.Title = model.Title;
                entityToUpdate.Subtitle = model.Subtitle;
                entityToUpdate.ImagePath = newImagePath; // Həmişə yeni və ya köhnə şəkil yolu ilə yenilənir

                entityToUpdate.Icon1 = model.Icon1;
                entityToUpdate.Title1 = model.Title1;
                entityToUpdate.Subtitle1 = model.Subtitle1;

                entityToUpdate.Icon2 = model.Icon2;
                entityToUpdate.Title2 = model.Title2;
                entityToUpdate.Subtitle2 = model.Subtitle2;

                entityToUpdate.Icon3 = model.Icon3;
                entityToUpdate.Title3 = model.Title3;
                entityToUpdate.Subtitle3 = model.Subtitle3;

                entityToUpdate.Icon4 = model.Icon4;
                entityToUpdate.Title4 = model.Title4;
                entityToUpdate.Subtitle4 = model.Subtitle4;

                try
                {
                    _context.Entry(entityToUpdate).State = EntityState.Modified; // Entity-nin state-ni dəyişdir
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("About section ID {Id} updated successfully.", entityToUpdate.Id);
                    TempData["SuccessMessage"] = "'Haqqımızda' məlumatı uğurla yeniləndi.";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Concurrency error updating About section ID {Id}.", entityToUpdate.Id);
                    // Yenidən fetch edib istifadəçiyə göstərmək və ya xəta mesajı vermək
                    if (!await AboutExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Məlumat başqa bir istifadəçi tərəfindən dəyişdirilib. Zəhmət olmasa səhifəni yeniləyin.");
                        // Köhnə image path-i qorumaq üçün model-i yenidən yükləyib view-a göndərmək olar
                        var currentValues = await _context.Abouts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.Id);
                        if (currentValues != null) model.ImagePath = currentValues.ImagePath;
                        return View(model);
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // ModelState valid deyilsə, köhnə şəkil yolunu qorumaq üçün
            if (string.IsNullOrEmpty(model.ImagePath))
            {
                var aboutFromDbForError = await _context.Abouts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == model.Id);
                if (aboutFromDbForError != null)
                {
                    model.ImagePath = aboutFromDbForError.ImagePath;
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var modelToDelete = await _context.Abouts.FirstOrDefaultAsync(x => x.Id == id);
            if (modelToDelete == null) return NotFound();
            return View(modelToDelete);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var modelToDelete = await _context.Abouts.FindAsync(id);
            if (modelToDelete == null) return NotFound();

            try
            {
                if (!string.IsNullOrEmpty(modelToDelete.ImagePath) && !modelToDelete.ImagePath.Contains("default-about-image.png"))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, modelToDelete.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Image deleted for About section ID {Id}: {ImagePath}", id, fullPath);
                    }
                }

                _context.Abouts.Remove(modelToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("About section ID {Id} deleted successfully.", id);
                TempData["SuccessMessage"] = "'Haqqımızda' məlumatı uğurla silindi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting About section ID {Id}", id);
                TempData["ErrorMessage"] = "Silmə zamanı xəta baş verdi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> AboutExists(int id)
        {
            return await _context.Abouts.AnyAsync(e => e.Id == id);
        }
    }
}