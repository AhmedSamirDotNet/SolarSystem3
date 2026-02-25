using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository
{
    public class ProjectCardTranslationRepository : Repository<ProjectCardTranslation>, IProjectCardTranslationRepository
    {
        private readonly ApplicationDbContext _db;
        public ProjectCardTranslationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ProjectCardTranslation entity)
        {
            var objFromDb = _db.ProjectCardTranslations.FirstOrDefault(s => s.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.LanguageCode = entity.LanguageCode;
                objFromDb.Title = entity.Title;
                objFromDb.LocationText = entity.LocationText;
                objFromDb.ProjectCardId = entity.ProjectCardId;
            }
        }
    }
}
