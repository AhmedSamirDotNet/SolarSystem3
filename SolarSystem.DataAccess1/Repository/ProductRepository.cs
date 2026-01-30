using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.DataAccess1.Repository
{
    
        public class ProductRepository : Repository<Product>, IProductRepository
        {
            private readonly ApplicationDbContext _db;
            public ProductRepository(ApplicationDbContext db) : base(db)
            {
                _db = db;
            }

        public void Update(Product entity)
        {
            var objFromDb = _db.Products.FirstOrDefault(p => p.Id == entity.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = entity.Name;
                objFromDb.MainDesc = entity.MainDesc;
                objFromDb.SubDesc = entity.SubDesc;
                objFromDb.Price = entity.Price;
                objFromDb.SectionId = entity.SectionId;
                // الصور بنحدثها لوحدها في الـ Controller عشان فيها شغل Files 📁
            }
        }
    }
    }

