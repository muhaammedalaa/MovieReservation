using MovieReservation.Data.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Data.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAsync(ISpecification<T> specification);
        Task<T?> GetSingleAsync(ISpecification<T> specification);
        Task<int> CountAsync(ISpecification<T> specification);
        IQueryable<T> GetQueryable(ISpecification<T> specification);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

    }
}
