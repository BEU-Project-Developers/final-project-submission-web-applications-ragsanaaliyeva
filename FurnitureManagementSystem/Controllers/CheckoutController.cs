using Microsoft.AspNetCore.Mvc;

namespace FurnitureManagementSystem.Controllers
{
	public class CheckoutController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
