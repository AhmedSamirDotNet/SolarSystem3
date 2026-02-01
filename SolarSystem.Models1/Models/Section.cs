using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Models1.Models
{
    public class Section
    {
        public int Id { get; set; }

        // الربط مع المنتجات
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // الربط مع جدول الترجمة (عربي وانجليزي)
        public ICollection<SectionTranslation> Translations { get; set; } = new List<SectionTranslation>();
    }
}
