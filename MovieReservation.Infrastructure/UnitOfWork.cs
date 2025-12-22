using MovieReservation.Data.Contracts;
using MovieReservation.Infrastructure.Data.Context;
using MovieReservation.Infrastructure.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieReservation.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;

        }
        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_dbContext);
                _repositories[type] = repositoryInstance;
            }
            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync() => await _dbContext.SaveChangesAsync();

    }
}
