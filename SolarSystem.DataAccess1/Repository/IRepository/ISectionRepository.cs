using SolarSystem.Models1.Models;
namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface ISectionRepository : IRepository<Section>
    {
        void Update(Section entity);
    }
}
