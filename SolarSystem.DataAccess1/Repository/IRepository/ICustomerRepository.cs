using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        void Update(Customer entity);
    }
}
