using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using System.Linq;

namespace SolarSystem.DataAccess1.Repository
{
    public class CustomerTranslationRepository : Repository<CustomerTranslation>, ICustomerTranslationRepository
    {
        private readonly ApplicationDbContext _db;
        public CustomerTranslationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CustomerTranslation entity)
        {
            _db.CustomerTranslations.Update(entity);
        }
    }
}
