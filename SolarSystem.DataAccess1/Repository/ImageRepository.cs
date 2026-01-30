using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;


namespace SolarSystem.DataAccess1.Repository
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private readonly ApplicationDbContext _db;
        public ImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Models1.Models.Image entity)
        {
            var objFromDb = _db.Images.FirstOrDefault(s => s.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.RelativePath = entity.RelativePath;
                if (objFromDb.RelativePath != null)
                {
                    objFromDb.RelativePath = entity.RelativePath;
                }
            }
        }
    }
}
