using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MainDesc { get; set; }
        public string? SubDesc { get; set; }
        public decimal Price { get; set; }

        [ForeignKey("Section")]
        public int SectionId { get; set; }
        public Section? Section { get; set; }

        public ICollection<Image> Images { get; set; } = new List<Image>();
    }
}