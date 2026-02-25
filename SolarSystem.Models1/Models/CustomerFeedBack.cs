using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Models1.Models
{
    public class CustomerFeedBack
    {
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<CustomerFeedbackTranslation> Translations { get; set; } = new List<CustomerFeedbackTranslation>();
    }
}
