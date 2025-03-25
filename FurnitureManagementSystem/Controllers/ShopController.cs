using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
	public class ShopController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
