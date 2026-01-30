using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.DataAccess1.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db) //implement this constructor to initialize the _CategoryRepository field and dbSet from child classes when objects are created
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> Filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            if (!string.IsNullOrEmpty(includeProperties)) // Fix: Check for non-null/non-empty
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp); // Eager loading of related entities
                }
            }

            query = query.Where(Filter); // Applies the filter to the query
            return query.FirstOrDefault(); // Executes the query and returns the first result or null
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? Filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet; // Defines a query related to this table

            if (Filter != null)
                query = query.Where(Filter); // Applies the filter to the query

            if (!string.IsNullOrEmpty(includeProperties)) // Fix: Check for non-null/non-empty
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp); // Eager loading of related entities
                }
            }

            return query.ToList(); // Executes the query and returns the list of results
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
