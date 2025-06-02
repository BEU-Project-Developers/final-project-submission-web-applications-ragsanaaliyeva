// ViewModels/CheckoutViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FurnitureManagementSystem.ViewModels
{
    public class CheckoutViewModel
    {
        // ... (FirstName, LastName, Email, Address, City, Country, PostalCode, PhoneNumber kimi göndərmə məlumatları [Required] olaraq qalmalıdır) ...

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        public string? Address2 { get; set; } // Opsional

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Postal code is required.")]
        public string PostalCode { get; set; }


        // Sifariş Xülasəsi
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal OrderTotal { get; set; }

        // BU SAHƏLƏRİ VƏ ONLARIN VALIDASIYA ATRIBUTLARINI SİLİN VƏ YA ŞƏRHƏ SALIN:
        // [Required(ErrorMessage = "The Card Number field is required.")] // <-- SİLİN və ya ŞƏRHƏ SALIN
        // [CreditCard] // <-- SİLİN və ya ŞƏRHƏ SALIN
        // public string CardNumber { get; set; }

        // [Required(ErrorMessage = "The CVV field is required.")] // <-- SİLİN və ya ŞƏRHƏ SALIN
        // [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")] // <-- SİLİN və ya ŞƏRHƏ SALIN
        // public string CVV { get; set; }

        // [Required(ErrorMessage = "The Expiry Date (MM/YY) field is required.")] // <-- SİLİN və ya ŞƏRHƏ SALIN
        // [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Expiry date must be in MM/YY format.")] // <-- SİLİN və ya ŞƏRHƏ SALIN
        // public string ExpiryDate { get; set; }
    }
}