using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
