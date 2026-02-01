using SolarSystem.DataAccess1.Repository.IRepository;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Product { get; }
        IImageRepository Image { get; }
        ISectionRepository Section { get; }
        IProductTranslationRepository ProductTranslation { get; }
        ISectionTranslationRepository SectionTranslation { get; }
        IAdminRepository Admin { get; }

        void Save(); // هي دي اللي بتعمل DbContext.SaveChanges()
    }
}