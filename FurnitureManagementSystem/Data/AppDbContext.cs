using FurnitureManagementSystem.Models; // Bütün modelləriniz üçün
using Microsoft.EntityFrameworkCore;

namespace FurnitureManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Autentifikasiya və Avtorizasiya üçün
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Digər layihə modelləriniz
        public DbSet<Home> Homes { get; set; } = null!;
        public DbSet<Products> Products { get; set; } = null!; // Model adınız Products idi
        public DbSet<About> Abouts { get; set; } = null!;    // Model adınız About idi
        public DbSet<Models.Services> Services { get; set; } = null!; // Model adınız Services idi, namespace konflikti olmasın deyə
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Team> Teams { get; set; } = null!; // Team modelini əlavə etdim (əgər varsa)
        public DbSet<Testimonial> Testimonials { get; set; } = null!; // Testimonial modelini əlavə etdim (əgər varsa)
        public DbSet<UserDetails> UserDetails { get; set; } = null!;
        public DbSet<Contact> Contacts { get; set; } = null!;
        
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Profession> Professions { get; set; } = null!;

        // ...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Bu sətir həmişə birinci olmalıdır

            // User və Role arasında çox-çoxa (many-to-many) əlaqə UserRole vasitəsilə
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles) // Role modelində public virtual ICollection<UserRole> UserRoles { get; set; } olmalıdır
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User modelinin konfiqurasiyası
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(e => e.AccountBalance)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0.00m); // Başlanğıc dəyəri

                entity.HasMany(u => u.Orders)
                      .WithOne(o => o.User)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Və ya Cascade, layihə tələblərinə görə

                // User və Cart arasında birə-bir əlaqə
                entity.HasOne(u => u.Cart)
                      .WithOne(c => c.User)
                      .HasForeignKey<Cart>(c => c.UserId);
            });

            // Role modelinin konfiqurasiyası
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique(); // RoleName yerinə Name olmalıdır (Role modelinizə baxın)
                                                         // Əgər RoleName-dirsə, r.RoleName olmalıdır.
                                                         // Sizin Role modelinizə görə Name düzgündür.
            });

            // Cart modelinin konfiqurasiyası
            modelBuilder.Entity<Cart>(entity =>
            {
                // Əgər Cart modelində TotalAmount propertisi varsa:
                entity.Property(c => c.TotalAmount)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0.00m); // Başlanğıc dəyəri (əgər varsa)
            });

            // Order modelinin konfiqurasiyası
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.OrderTotal)
                      .HasColumnType("decimal(18,2)");
            });

            // OrderItem modelinin konfiqurasiyası
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(18,2)");
            });

            // Products modelinin konfiqurasiyası
            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");
            });

            // Digər modelləriniz üçün əlavə konfiqurasiyalar (lazım olarsa)...
        }
    }
}