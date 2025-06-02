// Models/Order.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureManagementSystem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } // User modelinizin Id tipi ilə eyni olmalıdır
        public virtual User User { get; set; } // User modelinizə naviqasiya

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }

        [Required]
        [MaxLength(100)]
        public string Status { get; set; } // Məsələn: "Pending", "Processing", "Shipped", "Delivered", "Cancelled"

        // Göndərmə Məlumatları
        [Required]
        [MaxLength(100)]
        public string ShippingFirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string ShippingLastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string ShippingEmail { get; set; }

        [Required]
        [Phone]
        [MaxLength(30)]
        public string ShippingPhoneNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string ShippingAddressLine1 { get; set; }

        [MaxLength(200)]
        public string? ShippingAddressLine2 { get; set; } // Opsional

        [Required]
        [MaxLength(100)]
        public string ShippingCity { get; set; }

        [Required]
        [MaxLength(100)]
        public string ShippingCountry { get; set; }

        [Required]
        [MaxLength(20)]
        public string ShippingPostalCode { get; set; }

        // Ödənişlə bağlı məlumatlar (NÜMUNƏ ÜÇÜN VƏ ÇOX SADƏLƏŞDİRİLMİŞ)
        // Real tətbiqlərdə ödəniş məlumatları belə saxlanılmır.
        // Bu sahələr əvəzinə PaymentTransactionId kimi bir şey ola bilər.
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // Məsələn: "Credit Card (Simulated)"
        public string? PaymentCardLastFourDigits { get; set; } // Yalnız son 4 rəqəm (əgər saxlanılırsa)
        public string? PaymentTransactionReference { get; set; } // Əgər bir ödəniş sistemi ilə inteqrasiya varsa


        // Naviqasiya propertisi: Bu sifarişə aid olan məhsullar
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}