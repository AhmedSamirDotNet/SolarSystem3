using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IProjectCardTranslationRepository : IRepository<ProjectCardTranslation>
    {
        void Update(ProjectCardTranslation entity);
    }
}
