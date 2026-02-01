using System.Linq;
using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository
{
    public class ProductTranslationRepository : Repository<ProductTranslation>, IProductTranslationRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductTranslationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductTranslation entity)
        {
            var obj = _db.Set<ProductTranslation>().FirstOrDefault(t => t.Id == entity.Id);
            if (obj != null)
            {
                obj.LanguageCode = entity.LanguageCode;
                obj.Name = entity.Name;
                obj.MainDesc = entity.MainDesc;
                obj.SubDesc = entity.SubDesc;
                obj.ProductId = entity.ProductId;
            }
        }
    }
}
