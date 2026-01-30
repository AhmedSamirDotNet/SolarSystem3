using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string RelativePath { get; set; } = string.Empty;

        [ForeignKey("Product")] // Links this ID to the Product property below
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}