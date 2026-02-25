using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Models1.Models
{
    public class ProjectHomePageCard
    {
        public int Id { get; set; }
        public string ImageRelativePath { get; set; } = string.Empty;

        public virtual ICollection<ProjectCardTranslation> Translations { get; set; } = new List<ProjectCardTranslation>();
    }
}
