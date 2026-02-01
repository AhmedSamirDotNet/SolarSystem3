using SolarSystem.Models1.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class ProductTranslation
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = "en"; // 'ar' or 'en'

        public string Name { get; set; } = string.Empty;
        public string? MainDesc { get; set; }
        public string? SubDesc { get; set; }

        // Foreign Key للمنتج الأصلي
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}