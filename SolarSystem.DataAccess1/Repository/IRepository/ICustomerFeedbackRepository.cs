using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface ICustomerFeedbackRepository : IRepository<CustomerFeedBack>
    {
        void Update(CustomerFeedBack entity);
    }
}
