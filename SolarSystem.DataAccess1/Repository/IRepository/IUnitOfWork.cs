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
        IProjectHomePageCardRepository ProjectCard { get; }
        IProjectCardTranslationRepository ProjectCardTranslation { get; }
        ICustomerRepository Customer { get; }
        ICustomerFeedbackRepository CustomerFeedback { get; }
        ICustomerTranslationRepository CustomerTranslation { get; }
        ICustomerFeedbackTranslationRepository CustomerFeedbackTranslation { get; }

        void Save(); // هي دي اللي بتعمل DbContext.SaveChanges()
        Task SaveAsync();
    }
}