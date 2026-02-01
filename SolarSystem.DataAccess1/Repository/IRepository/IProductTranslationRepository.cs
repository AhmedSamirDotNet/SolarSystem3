using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IProductTranslationRepository : IRepository<ProductTranslation>
    {
        void Update(ProductTranslation entity);
    }
}
