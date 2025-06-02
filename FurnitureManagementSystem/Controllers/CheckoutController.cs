// FurnitureShopProjectRazil.Controllers.CheckoutController.cs
using FurnitureManagementSystem.Data;
using FurnitureManagementSystem.Models;
using FurnitureManagementSystem.ViewModels; // CheckoutViewModel üçün
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FurnitureManagementSystem.Controllers
{
    [Authorize] // Bu controller-ə yalnız autentifikasiya olunmuş istifadəçilər daxil ola bilər
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(AppDbContext context, ILogger<CheckoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Köməkçi metod: Cari istifadəçinin ID-sini alır (int olaraq)
        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out userId))
            {
                _logger.LogWarning("CheckoutController: User ID not found in claims or is not a valid integer for an authorized user. Claim Value: '{UserIdClaim}'", userIdStr);
                return false;
            }
            return true;
        }

        // Köməkçi metod: Cari istifadəçinin səbətini alır
        private async Task<Cart?> GetCurrentUserCartAsync()
        {
            if (!TryGetUserId(out int userId)) return null;

            return await _context.Carts
                                 .Include(c => c.Items)
                                 .ThenInclude(i => i.Product) // Məhsul məlumatları View üçün lazımdır
                                 .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // GET: /Checkout/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TryGetUserId(out int userId))
            {
                TempData["ErrorMessage"] = "Please log in to proceed to checkout.";
                return RedirectToAction("Login", "Account"); // AccountController-dəki Login action-ı
            }

            var cart = await GetCurrentUserCartAsync();
            if (cart == null || !cart.Items.Any())
            {
                TempData["InfoMessage"] = "Your cart is empty. Please add items to your cart before proceeding to checkout.";
                return RedirectToAction("Index", "Cart"); // Səbət səhifəsinə yönləndir
            }

            var user = await _context.Users
                                     .Include(u => u.UserDetails) // Əgər UserDetails varsa və oradan məlumat alacaqsınızsa
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogError("Checkout GET: User with ID {UserId} not found in database, but claims exist.", userId);
                TempData["ErrorMessage"] = "User account could not be retrieved. Please try logging in again.";
                return RedirectToAction("Login", "Account");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                CartItems = cart.Items.Select(ci => new CartItemViewModel
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Title,
                    ProductImagePath = ci.Product.ImagePath,
                    Price = ci.UnitPrice,
                    Quantity = ci.Quantity
                }).ToList(),
                // İstifadəçi məlumatlarını CheckoutViewModel-ə дефаулт olaraq ötürək
                FirstName = user.FullName?.Split(' ').FirstOrDefault() ?? "",
                LastName = user.FullName?.Split(' ').LastOrDefault() ?? user.FullName ?? "", // Əgər tək addırsa
                Email = user.Email,
                // ViewModel-inizdə bu sahələr olmalıdır:
                // PhoneNumber = user.UserDetails?.PhoneNumber ?? user.PhoneNumber, // UserDetails və ya User-dən
                // Address = user.UserDetails?.AddressLine1 ?? "",
                // Address2 = user.UserDetails?.AddressLine2,
                // City = user.UserDetails?.City ?? "",
                // Country = user.UserDetails?.Country ?? "",
                // PostalCode = user.UserDetails?.PostalCode ?? ""
            };
            checkoutViewModel.OrderTotal = checkoutViewModel.CartItems.Sum(item => item.TotalPrice);

            ViewBag.CurrentUserBalance = user.AccountBalance; // İstifadəçinin balansını View-a ötürürük

            return View(checkoutViewModel);
        }

        // POST: /Checkout/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(CheckoutViewModel model)
        {
            if (!TryGetUserId(out int userId))
            {
                TempData["ErrorMessage"] = "Authentication error. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User account not found.";
                _logger.LogError("ProcessOrder POST: User with ID {UserId} not found in database after successful GetUserId.", userId);
                return RedirectToAction("Login", "Account");
            }

            var cart = await GetCurrentUserCartAsync();

            // Xəta halında View-a qaytarmaq üçün ViewModel-i və ViewBag-i yenidən doldururuq
            if (cart != null && cart.Items.Any())
            {
                // Bu məlumatlar onsuz da model binding ilə gəlir,
                // amma səbət boşalarsa və ya ModelState xətası olarsa, CartItems-i yenidən təyin etmək lazımdır.
                model.CartItems = cart.Items.Select(ci => new CartItemViewModel
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Title,
                    ProductImagePath = ci.Product.ImagePath,
                    Price = ci.UnitPrice,
                    Quantity = ci.Quantity
                }).ToList();
                model.OrderTotal = model.CartItems.Sum(item => item.TotalPrice);
            }
            else
            {
                ModelState.AddModelError("", "Your cart is currently empty or could not be retrieved. Please add items to your cart.");
                model.CartItems ??= new List<CartItemViewModel>(); // Null olmasının qarşısını al
                model.OrderTotal = 0;
            }
            ViewBag.CurrentUserBalance = user.AccountBalance; // Həmişə balansı View-a ötürək


            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ProcessOrder: ModelState is invalid for user {UserId}. Errors: {Errors}", userId,
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View("Index", model); // Xətalarla birlikdə Index View-una qayıdırıq
            }

            // 1. Balans Yoxlanışı
            if (user.AccountBalance < model.OrderTotal)
            {
                _logger.LogWarning("ProcessOrder: Insufficient balance for user {UserId}. Required: {RequiredAmount}, Available: {AvailableAmount}",
                    userId, model.OrderTotal, user.AccountBalance);
                ModelState.AddModelError("", $"Your account balance ({user.AccountBalance:C}) is insufficient for this order ({model.OrderTotal:C}). Please add funds to your account.");
                return View("Index", model);
            }

            // === Transaksiya Başladılır ===
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Order Obyekti Yaradılır
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    OrderTotal = model.OrderTotal,
                    Status = "Processing", // Başlanğıc status

                    // Göndərmə məlumatları CheckoutViewModel-dən Order modelinə köçürülür
                    ShippingFirstName = model.FirstName,
                    ShippingLastName = model.LastName,
                    ShippingEmail = model.Email,
                    ShippingPhoneNumber = model.PhoneNumber,    // ViewModel-də olmalıdır
                    ShippingAddressLine1 = model.Address,       // ViewModel-də olmalıdır
                    ShippingAddressLine2 = model.Address2,      // ViewModel-də olmalıdır (opsional)
                    ShippingCity = model.City,              // ViewModel-də olmalıdır
                    ShippingCountry = model.Country,          // ViewModel-də olmalıdır
                    ShippingPostalCode = model.PostalCode,      // ViewModel-də olmalıdır

                    PaymentMethod = "Account Balance" // Ödəniş metodu
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // OrderId-ni almaq üçün birinci save
                _logger.LogInformation("Order {OrderId} created for user {UserId}. OrderTotal: {OrderTotal}", order.Id, userId, order.OrderTotal);

                // 3. OrderItem Obyektləri Yaradılır
                if (cart == null || !cart.Items.Any()) // Bu yoxlama artıq yuxarıda edilib, amma ehtiyat üçün
                {
                    _logger.LogError("ProcessOrder: Cart became null or empty unexpectedly before creating OrderItems for OrderId {OrderId}", order.Id);
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An error occurred with your cart during order processing. Please try again.");
                    return View("Index", model);
                }

                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice // Səbətdəki qiymət
                    };
                    _context.OrderItems.Add(orderItem);
                }
                // OrderItems üçün ayrıca SaveChanges-ə ehtiyac yoxdur, sonda hamısını birlikdə edəcəyik.

                // 4. İstifadəçinin Balansı Azaldılır
                decimal previousBalance = user.AccountBalance;
                user.AccountBalance -= model.OrderTotal;
                // EF Core user obyektini izlədiyi üçün _context.Users.Update(user) çağırmağa ehtiyac yoxdur.
                _logger.LogInformation("User {UserId} account balance to be updated. Previous: {PreviousBalance}, Debited: {DebitedAmount}, New Expected: {NewBalance}",
                    userId, previousBalance, model.OrderTotal, user.AccountBalance);

                // 5. Səbət Təmizlənir
                _context.CartItems.RemoveRange(cart.Items); // Bütün elementləri sil
                cart.TotalAmount = 0; // Səbətin özündəki məbləği də sıfırla
                cart.LastModifiedDate = DateTime.UtcNow;
                // EF Core cart obyektini izlədiyi üçün _context.Carts.Update(cart) çağırmağa ehtiyac yoxdur.

                // Bütün dəyişikliklər (OrderItems, User.AccountBalance, Cart.TotalAmount, CartItems.Remove) bazaya yazılır
                await _context.SaveChangesAsync();

                await transaction.CommitAsync(); // Transaksiya təsdiqlənir
                _logger.LogInformation("Order {OrderId} processed successfully using account balance for user {UserId}. Cart cleared. Transaction committed.", order.Id, userId);

                // 6. Uğur Səhifəsinə Yönləndirilir
                TempData["SuccessMessage"] = "Your order has been placed successfully using your account balance!";
                TempData["OrderId"] = order.Id; // Sifariş ID-sini ThankYou səhifəsinə ötürmək üçün
                return RedirectToAction("Index", "ThankYou"); // ThankYouController-in Index action-ı

            }
            catch (DbUpdateException dbEx) // Verilənlər bazası ilə bağlı xətalar üçün
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "DbUpdateException while processing order for user {UserId}. Transaction rolled back. Model: {@CheckoutModel}", userId, model);
                ModelState.AddModelError("", "A database error occurred while processing your order. Please review your information and try again. If the problem persists, contact support.");
                return View("Index", model);
            }
            catch (Exception ex) // Gözlənilməyən digər xətalar üçün
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error while processing order for user {UserId}. Transaction rolled back. Model: {@CheckoutModel}", userId, model);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again or contact support.");
                return View("Index", model);
            }
        }
    }
}