using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IProjectHomePageCardRepository : IRepository<ProjectHomePageCard>
    {
        void Update(ProjectHomePageCard entity);
    }
}
