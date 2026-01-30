using SolarSystem.Models1.Models;
using SolarSystem.Models1.Models;


namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IAdminRepository : IRepository<Admin>
    {
        void Update(Admin entity);
    }
}
