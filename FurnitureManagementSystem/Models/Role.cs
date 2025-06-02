using System.ComponentModel.DataAnnotations;

namespace FurnitureManagementSystem.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rol adı tələb olunur.")]
        [StringLength(50)]
        public string Name { get; set; } 
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
