using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class CustomerTranslation
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = "en"; // 'ar' or 'en'
        
        public string Name { get; set; } = string.Empty;
        public string? Job { get; set; }

        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }
    }
}
