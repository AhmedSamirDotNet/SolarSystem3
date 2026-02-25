using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface ICustomerFeedbackTranslationRepository : IRepository<CustomerFeedbackTranslation>
    {
        void Update(CustomerFeedbackTranslation entity);
    }
}
