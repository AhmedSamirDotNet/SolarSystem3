using SolarSystem.Models1.Models;


namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IProductRepository : IRepository<Models1.Models.Product>
    {
        void Update(Models1.Models.Product entity);
    }
}
