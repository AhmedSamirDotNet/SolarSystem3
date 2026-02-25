using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using System.Linq;

namespace SolarSystem.DataAccess1.Repository
{
    public class CustomerFeedbackTranslationRepository : Repository<CustomerFeedbackTranslation>, ICustomerFeedbackTranslationRepository
    {
        private readonly ApplicationDbContext _db;
        public CustomerFeedbackTranslationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CustomerFeedbackTranslation entity)
        {
            _db.CustomerFeedbackTranslations.Update(entity);
        }
    }
}
