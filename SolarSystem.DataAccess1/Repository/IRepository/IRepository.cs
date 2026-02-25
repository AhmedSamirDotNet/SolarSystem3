using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SolarSystem.DataAccess1.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? Filter = null, string? includeProperties = null);
        Task<T> GetAsync(Expression<Func<T, bool>>? Filter = null, string? includeProperties = null, bool tracked = false);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        
        // Backward compatibility
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? Filter = null, string? includeProperties = null);
        T Get(Expression<Func<T, bool>>? Filter = null, string? includeProperties = null, bool tracked = false);
    }
}
