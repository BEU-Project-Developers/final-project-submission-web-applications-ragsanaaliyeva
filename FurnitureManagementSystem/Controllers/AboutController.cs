using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
	public class AboutController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
