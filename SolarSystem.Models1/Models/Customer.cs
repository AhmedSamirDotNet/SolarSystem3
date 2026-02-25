using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Models1.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
  
        public virtual ICollection<CustomerFeedBack> CustomerFeedBacks { get; set; } = new List<CustomerFeedBack>();
        public virtual ICollection<CustomerTranslation> Translations { get; set; } = new List<CustomerTranslation>();
    }
}
