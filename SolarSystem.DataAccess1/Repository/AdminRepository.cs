using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;


namespace SolarSystem.DataAccess1.Repository
{
    public class AdminRepository : Repository<Admin>, IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Admin entity)
        {
            var objFromDb = _db.Admins.FirstOrDefault(s => s.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Username = entity.Username;
                objFromDb.Role = entity.Role; // مهم جداً للتحديث 🔑
                if (!string.IsNullOrEmpty(entity.PasswordHash))
                {
                    objFromDb.PasswordHash = entity.PasswordHash;
                }
            }
        }
    }
        }

