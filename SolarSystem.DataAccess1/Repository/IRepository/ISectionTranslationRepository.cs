
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface ISectionTranslationRepository : IRepository<SectionTranslation>
    {
        void Update(SectionTranslation entity);
    }
}
