using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IImageRepository : IRepository<Models1.Models.Image>
    {
        void Update(Models1.Models.Image entity);
    }
}
