// Models/User.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Column atributu üçün

namespace FurnitureManagementSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı tələb olunur.")]
        [StringLength(100, ErrorMessage = "İstifadəçi adı 3-100 simvol aralığında olmalıdır.", MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "E-poçt ünvanı tələb olunur.")]
        [EmailAddress(ErrorMessage = "Düzgün e-poçt ünvanı daxil edin.")]
        [StringLength(256)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tam ad tələb olunur.")]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryDate { get; set; } // PasswordResetTokenExpiry olaraq dəyişə bilərsiniz

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? ImagePath { get; set; }

        // YENİ PROPERTY: İstifadəçinin virtual kart hesabı/balansı
        [Column(TypeName = "decimal(18,2)")]
        public decimal AccountBalance { get; set; } = 500m;

        // Naviqasiya Propertiləri
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual UserDetails? UserDetails { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>(); // Sifarişlər üçün əlaqə
        public virtual Cart? Cart { get; set; } // Bir istifadəçinin bir səbəti ola bilər (əgər belə qurulubsa)
    }
}