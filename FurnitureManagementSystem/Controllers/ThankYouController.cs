// Controllers/ThankYouController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Səhifəyə yalnız daxil olmuş istifadəçilər baxsın

namespace FurnitureManagementSystem.Controllers
{
    [Authorize] // Bu səhifəyə yalnız autentifikasiya olunmuş istifadəçilər daxil ola bilər
    public class ThankYouController : Controller
    {
        public IActionResult Index()
        {
            // CheckoutController-dən gələn TempData-ları yoxlayırıq
            if (TempData["OrderId"] == null || TempData["SuccessMessage"] == null)
            {
                // Əgər sifariş məlumatı yoxdursa (məsələn, birbaşa URL ilə gəlibsə),
                // istifadəçini ana səhifəyə və ya dükan səhifəsinə yönləndiririk.
                TempData["InfoMessage"] = "No order information found."; // Opsional məlumat mesajı
                return RedirectToAction("Index", "Shop"); // Və ya "Home"
            }

            ViewBag.OrderId = TempData["OrderId"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            // TempData-nı təmizləmək üçün Peek əvəzinə birbaşa istifadə etdik,
            // beləliklə səhifə yeniləndikdə təkrar görünməyəcək.
            // Əgər təkrar istifadə lazım olsaydı, TempData.Keep() istifadə edilə bilərdi.
            return View(); // Views/ThankYou/Index.cshtml
        }
    }
}