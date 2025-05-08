using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using FurnitureManagementSystem.Models;
using System.IO;
using System.Threading.Tasks;

namespace FurnitureManagementSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AboutUsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public AboutUsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Simulyasiya üçün sadə yaddaşda saxlanan məlumatlar
        private static List<About> aboutList = new List<About>
        {
            new About
            {
                Id = 1,
                Title = "Voluptatem dignissimos provident quasi corporis",
                Subtitle = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                //Description1 = "Ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                //Description2 = "Duis aute irure dolor in reprehenderit in voluptate velit.",
                //Description3 = "Duis aute irure dolor in reprehenderit in voluptate velit.",
                ImagePath = "admin/assets/about-2.jpg"
            }
        };

        public IActionResult Index()
        {
            return View(aboutList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(About model)
        {
            if (model.Photo != null && model.Photo.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "admin", "assets");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                model.ImagePath = $"admin/assets/{fileName}";
            }

            model.Id = aboutList.Max(x => x.Id) + 1;
            aboutList.Add(model);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Update(int id)
        {
            var model = aboutList.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(About model)
        {
            var existing = aboutList.FirstOrDefault(x => x.Id == model.Id);
            if (existing == null) return NotFound();

            if (model.Photo != null && model.Photo.Length > 0)
            {
                string folderPath = Path.Combine(_env.WebRootPath, "admin", "assets");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                existing.ImagePath = $"admin/assets/{fileName}";
            }

            // Digər sahələri yenilə
            existing.Title = model.Title;
            existing.Subtitle = model.Subtitle;
            //existing.Description1 = model.Description1;
            //existing.Description2 = model.Description2;
            //existing.Description3 = model.Description3;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var model = aboutList.FirstOrDefault(x => x.Id == id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(About model)
        {
            var item = aboutList.FirstOrDefault(x => x.Id == model.Id);
            if (item != null)
            {
                aboutList.Remove(item);
            }

            return RedirectToAction("Index");
        }

    }
}
