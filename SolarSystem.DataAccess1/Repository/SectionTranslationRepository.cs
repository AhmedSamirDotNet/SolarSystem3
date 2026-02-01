using System.Linq;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository
{
    public class SectionTranslationRepository : Repository<SectionTranslation>, ISectionTranslationRepository
    {
        private readonly ApplicationDbContext _db;
        public SectionTranslationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(SectionTranslation entity)
        {
            var obj = _db.Set<SectionTranslation>().FirstOrDefault(t => t.Id == entity.Id);
            if (obj != null)
            {
                obj.LanguageCode = entity.LanguageCode;
                obj.Name = entity.Name;
                obj.SectionId = entity.SectionId;
            }
        }
    }
}
