using SolarSystem.DataAccess1.Repository.IRepository;
using SolarSystem.Models1.Models;

namespace SolarSystem.DataAccess1.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IProductRepository Product { get; private set; }
        public IImageRepository Image { get; private set; }
        public ISectionRepository Section { get; private set; }
        public IProductTranslationRepository ProductTranslation { get; private set; }
        public ISectionTranslationRepository SectionTranslation { get; private set; }
        public IAdminRepository Admin { get; private set; }
        public IProjectHomePageCardRepository ProjectCard { get; private set; }
        public IProjectCardTranslationRepository ProjectCardTranslation { get; private set; }
        public ICustomerRepository Customer { get; private set; }
        public ICustomerFeedbackRepository CustomerFeedback { get; private set; }
        public ICustomerTranslationRepository CustomerTranslation { get; private set; }
        public ICustomerFeedbackTranslationRepository CustomerFeedbackTranslation { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            // بنبعت نفس الـ _db لكل ريبوزيتوري عشان نضمن إنهم شغالين في نفس الـ Transaction
            Product = new ProductRepository(_db);
            Image = new ImageRepository(_db);
            Section = new SectionRepository(_db);
            ProductTranslation = new ProductTranslationRepository(_db);
            SectionTranslation = new SectionTranslationRepository(_db);
            Admin = new AdminRepository(_db);
            ProjectCard = new ProjectHomePageCardRepository(_db);
            ProjectCardTranslation = new ProjectCardTranslationRepository(_db);
            Customer = new CustomerRepository(_db);
            CustomerFeedback = new CustomerFeedbackRepository(_db);
            CustomerTranslation = new CustomerTranslationRepository(_db);
            CustomerFeedbackTranslation = new CustomerFeedbackTranslationRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}