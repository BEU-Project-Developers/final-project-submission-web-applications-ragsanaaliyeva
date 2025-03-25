using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
	public class ServicesController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
