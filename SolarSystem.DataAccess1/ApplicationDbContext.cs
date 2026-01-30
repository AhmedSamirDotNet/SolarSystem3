using Microsoft.EntityFrameworkCore;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1
{
    public class ApplicationDbContext : DbContext
    {
        // نمرر الـ options إلى الكلاس الأساسي (base) ليعرف نوع قاعدة البيانات والـ Connection String
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Admin>()
                .Property(a => a.Role)
                .HasConversion<string>();

            modelBuilder.Entity<Admin>().HasData(
                new Admin { Id = 1, Username = "master_chief", PasswordHash = "secret_hash", Role = AdminRole.MasterAdmin }
            );

            modelBuilder.Entity<Section>().HasData(
                new Section { Id = 1, Name = "Solar Panels" },
                new Section { Id = 2, Name = "Inverters" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Super Solar 3000",
                    MainDesc = "High efficiency panel",
                    Price = 500.00m,
                    SectionId = 1 
                },
                new Product
                {
                    Id = 2,
                    Name = "MaxVolt Inverter",
                    MainDesc = "Pure sine wave inverter",
                    Price = 1200.00m,
                    SectionId = 2 
                }
            );

            modelBuilder.Entity<Image>().HasData(
                new Image { Id = 1, RelativePath = "/images/panel1.png", ProductId = 1 },
                new Image { Id = 2, RelativePath = "/images/panel1.png", ProductId = 1 },
                new Image { Id = 3, RelativePath = "/images/inverter1.jpg", ProductId = 2 }
            );
        }
    }
}
