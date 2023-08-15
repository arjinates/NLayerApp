using Microsoft.EntityFrameworkCore;
using NLayer.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NLayer.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context; //readonly objelere ancak bu satirdaki gibi ilk
        private readonly DbSet<T> _dbSet;         //olusturulduklarında ya da constructure'da
                                                  //deger atayabiliriz

        public GenericRepository(AppDbContext context, DbSet<T> dbSet)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);

        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> exp)
        {
             return await _dbSet.AnyAsync(exp);
        }

        public IQueryable<T> GetAll()
        {
            return  _dbSet.AsNoTracking().AsQueryable();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void remove(T entity)
        {
             _dbSet.Remove(entity); //asenkron metodu yok, bu entity'nin state'sini deleted olarak koyuyor
                                    //yani bir flag koyuyor aslında, savaChanges() metodu cagrildiginda
        }                           //deleted flagleri bulup, db'den siliyor
                                    //yaptigi sey >> _context.Entry(entity).State=EntityState.Deleted; 
        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> exp)
        {
            return _dbSet.Where(exp);
        }
    }
}
