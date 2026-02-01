using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public decimal Price { get; set; }

        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public Section? Section { get; set; }

        public ICollection<Image> Images { get; set; } = new List<Image>();

        // الربط مع جدول ترجمة المنتجات
        public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();
    }
}