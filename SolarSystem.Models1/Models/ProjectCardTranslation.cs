using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Models1.Models
{
    public class ProjectCardTranslation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectCardId { get; set; }

        [ForeignKey("ProjectCardId")]
        public virtual ProjectHomePageCard? ProjectCard { get; set; }

        [Required]
        [StringLength(10)]
        public string LanguageCode { get; set; } = "en";

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string LocationText { get; set; } = string.Empty;
    }
}
