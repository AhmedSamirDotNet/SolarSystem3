using System.ComponentModel.DataAnnotations.Schema;

namespace SolarSystem.Models1.Models
{
    public class CustomerFeedbackTranslation
    {
        public int Id { get; set; }
        public string LanguageCode { get; set; } = "en"; // 'ar' or 'en'

        public string FeedbackText { get; set; } = string.Empty;

        public int CustomerFeedbackId { get; set; }
        [ForeignKey("CustomerFeedbackId")]
        public CustomerFeedBack? CustomerFeedback { get; set; }
    }
}
