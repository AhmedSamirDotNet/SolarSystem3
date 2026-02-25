using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using System.Linq;

namespace SolarSystem.DataAccess1.Repository
{
    public class CustomerFeedbackRepository : Repository<CustomerFeedBack>, ICustomerFeedbackRepository
    {
        private readonly ApplicationDbContext _db;
        public CustomerFeedbackRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CustomerFeedBack entity)
        {
            var objFromDb = _db.CustomerFeedBacks.FirstOrDefault(p => p.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.CustomerId = entity.CustomerId;
            }
        }
    }
}
