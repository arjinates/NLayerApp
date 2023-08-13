using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NLayer.Core.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);

        IQueryable<T> GetAll();

        //IQueryable donen seylerde veritabanına sorgu atilmaz, toList'e kadar olan tum kosullar
        //memory'de birlestirilir ve toList ile birlikte tek seferde veritabanına gönderilir

        // productRepository.Where(x=> x.Id>%).OrderBy.ToListAsync();
        IQueryable<T> Where(Expression<Func<T, bool>> exp);
        //Queryable kullanmamin sebebi henuz veritabanina gitmeden OrderBy gibi siralama yapabilmek icin
        //meaning, uygulamanin daha performansli calismasi icin

        Task<bool> AnyAsync(Expression<Func<T, bool>> exp);

        Task AddAsync(T entity);//async programlamayi var olan threadlari bloklamamak icin kullaniyoruz

        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity); //update ve remove'un async'i yok cunku uzun suren islemler olmadiklari icin

        void remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
