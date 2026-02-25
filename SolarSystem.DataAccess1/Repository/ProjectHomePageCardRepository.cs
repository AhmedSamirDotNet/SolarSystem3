using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository
{
    public class ProjectHomePageCardRepository : Repository<ProjectHomePageCard>, IProjectHomePageCardRepository
    {
        private readonly ApplicationDbContext _db;
        public ProjectHomePageCardRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ProjectHomePageCard entity)
        {
            var objFromDb = _db.ProjectHomePageCards.FirstOrDefault(s => s.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.ImageRelativePath = entity.ImageRelativePath;
            }
        }
    }
}
