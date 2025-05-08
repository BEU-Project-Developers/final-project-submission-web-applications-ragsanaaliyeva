using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
    public class TeamController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
