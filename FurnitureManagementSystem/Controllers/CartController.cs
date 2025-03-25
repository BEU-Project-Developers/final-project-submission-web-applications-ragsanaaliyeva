using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
