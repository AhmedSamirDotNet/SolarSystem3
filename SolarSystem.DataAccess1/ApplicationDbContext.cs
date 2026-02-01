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
        public DbSet<ProductTranslation> ProductTranslations { get; set; }
        public DbSet<SectionTranslation> SectionTranslations { get; set; }
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

            // Seed core entities (no Name/MainDesc here - translations store localized text)
            modelBuilder.Entity<Section>().HasData(
                new Section { Id = 1 },
                new Section { Id = 2 }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Price = 500.00m,
                    SectionId = 1
                },
                new Product
                {
                    Id = 2,
                    Price = 1200.00m,
                    SectionId = 2
                }
            );

            modelBuilder.Entity<SectionTranslation>().HasData(
                new SectionTranslation { Id = 1, LanguageCode = "en", Name = "Solar Panels", SectionId = 1 },
                new SectionTranslation { Id = 2, LanguageCode = "en", Name = "Inverters", SectionId = 2 },
                new SectionTranslation { Id = 3, LanguageCode = "ar", Name = "الألواح الشمسية", SectionId = 1 },
                new SectionTranslation { Id = 4, LanguageCode = "ar", Name = "المحولات", SectionId = 2 }
            );

            modelBuilder.Entity<ProductTranslation>().HasData(
                new ProductTranslation { Id = 1, LanguageCode = "en", Name = "Super Solar 3000", MainDesc = "High efficiency panel", ProductId = 1 },
                new ProductTranslation { Id = 2, LanguageCode = "en", Name = "MaxVolt Inverter", MainDesc = "Pure sine wave inverter", ProductId = 2 },
                new ProductTranslation { Id = 3, LanguageCode = "ar", Name = "سوبر سولار 3000", MainDesc = "لوح عالي الكفاءة", ProductId = 1 },
                new ProductTranslation { Id = 4, LanguageCode = "ar", Name = "ماكس فولت إنفرتر", MainDesc = "محول موجة جيبية نقية", ProductId = 2 }
            );

            modelBuilder.Entity<Image>().HasData(
                new Image { Id = 1, RelativePath = "/images/panel1.png", ProductId = 1 },
                new Image { Id = 2, RelativePath = "/images/panel1.png", ProductId = 1 },
                new Image { Id = 3, RelativePath = "/images/inverter1.jpg", ProductId = 2 }
            );
        }
    }
}
