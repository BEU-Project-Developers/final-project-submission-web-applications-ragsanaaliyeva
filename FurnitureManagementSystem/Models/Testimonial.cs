// Models/Testimonial.cs
using Microsoft.AspNetCore.Http; // IFormFile üçün
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurnitureManagementSystem.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad və soyad daxil edilməlidir.")]
        [MaxLength(100, ErrorMessage = "Ad və soyad 100 simvoldan çox ola bilməz.")]
        [Display(Name = "Ad Soyad")]
        public string AuthorFullName { get; set; }

        [Required(ErrorMessage = "Vəzifə daxil edilməlidir.")]
        [MaxLength(100, ErrorMessage = "Vəzifə 100 simvoldan çox ola bilməz.")]
        [Display(Name = "Vəzifə/Şirkət")] // "Şirkət" də əlavə etmək olar
        public string AuthorTitle { get; set; }

        [Required(ErrorMessage = "Rəy mətni daxil edilməlidir.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Rəy Mətni")]
        public string Content { get; set; }

        [Display(Name = "Şəkil")]
        public string? ImagePath { get; set; } // Rəy verənin şəklinin serverdəki yolu (məs: "uploads/testimonials/image.jpg")
                                               // Nullable etdim, şəkil olmaya da bilər.

        [NotMapped] // Bu xüsusiyyət verilənlər bazasına yazılmayacaq
        [Display(Name = "Yeni Şəkil Yüklə")]
        public IFormFile? Photo { get; set; } // Şəkil yükləmək üçün (nullable)

        [Display(Name = "Paylaşılma Tarixi")]
        [DataType(DataType.Date)] // Yalnız tarix göstərmək üçün
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

        [Display(Name = "Aktivdirmi?")]
        public bool IsActive { get; set; } = true; // Rəyin saytda göstərilib-göstərilməyəcəyi
    }
}