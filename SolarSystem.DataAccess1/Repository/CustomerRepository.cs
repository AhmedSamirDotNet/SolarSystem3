using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using System.Linq;

namespace SolarSystem.DataAccess1.Repository
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private readonly ApplicationDbContext _db;
        public CustomerRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Customer entity)
        {
            _db.Customers.Update(entity);
        }
    }
}
