using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class SectionTranslation
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = "en"; // 'ar' or 'en'
        public string Name { get; set; } = string.Empty;

        // Foreign Key للقسم الأصلي
        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public Section? Section { get; set; }
    }
}