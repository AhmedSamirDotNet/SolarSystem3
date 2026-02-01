using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;


namespace SolarSystem.DataAccess1.Repository
{
    public class SectionRepository : Repository<Section> , ISectionRepository
    {
        private readonly ApplicationDbContext _db;
        public SectionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Section entity)
        {
            var objFromDb = _db.Sections.FirstOrDefault(s => s.Id == entity.Id);
            if (objFromDb != null)
            {
                // Keep translations unchanged here; Translations are managed separately
                // If english translation exists as default, ensure there's at least one translation
                if (entity.Translations != null && entity.Translations.Any())
                {
                    // ensure translations collection is synchronized by caller
                }
            }
        }
    }
}
