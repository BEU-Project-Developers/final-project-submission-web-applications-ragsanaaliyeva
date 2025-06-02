// Models/Cart.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Column atributu üçün
// using System.Linq; // Əgər UpdateTotalAmount metodu burada olsaydı lazım olardı

namespace FurnitureManagementSystem.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }

        // BU PROPERTY MÖVCUD OLMALIDIR:
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0.00m; // Başlanğıc dəyəri

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Konstruktorlar (əgər varsa)
        public Cart() { }

        public Cart(int userId)
        {
            UserId = userId;
            CreatedDate = DateTime.UtcNow;
            TotalAmount = 0.00m; // Yeni səbət üçün
            Items = new List<CartItem>();
        }
    }
}