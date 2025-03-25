using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
	public class BlogController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
