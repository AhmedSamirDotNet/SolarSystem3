using SolarSystem.Models1.Models;


namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product entity);
    }
}
