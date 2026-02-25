using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface ICustomerTranslationRepository : IRepository<CustomerTranslation>
    {
        void Update(CustomerTranslation entity);
    }
}
