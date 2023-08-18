using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NLayer.Core.DTOs;
using NLayer.Core.Models;
using NLayer.Core.Repositories;
using NLayer.Core.Services;
using NLayer.Core.UnitOfWorks;
using NLayer.Service.Exceptions;
using NLayer.Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NLayer.Caching
{
    public class ProductServiceWithCaching : IProductService //decorator/proxy design pattern implementation
    {   //open-closed principle'e uygun. Degisime kapali ancak gelisime acik
        //var olan yapiyi bozmuyoruz ama yeni ozellik eklemek istedigimiz zaman da kolayca implement edebiliyoruz

        private readonly string CacheProductKey = "productsCache";
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;
        private readonly IProductRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductServiceWithCaching(IMapper mapper, IUnitOfWork unitOfWork, IProductRepository repository, IMemoryCache memoryCache)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _repository = repository;
            _memoryCache = memoryCache;

            if (!_memoryCache.TryGetValue(CacheProductKey, out _)) //sadece cache'de data var mi yok mu onu ogrenmek istiyoruz varsa data donsun 
            {                                                      //istemiyoruz, bu yüzden out _ kullandık memoryde bosuna allocate etmesin diye
                _memoryCache.Set(CacheProductKey, _repository.GetProductsWithCategory().Result); //yok ise memoryCache'e CacheProductKey'e repodaki datalari set et
            }
        }

        public async Task<Product> AddAsync(Product entity)
        {
            await _repository.AddAsync(entity);
            await _unitOfWork.CommitAsync();
            await CacheAllProductsAsync();   
            return entity;
        }

        public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities)
        {
            await _repository.AddRangeAsync(entities);
            await _unitOfWork.CommitAsync();
            await CacheAllProductsAsync();  
            return entities;
        }

        public Task<bool> AnyAsync(Expression<Func<Product, bool>> exp)
        {
            return Task.FromResult(_memoryCache.Get<List<Product>>(CacheProductKey).Any(exp.Compile()));
        }

        public  Task<IEnumerable<Product>> GetAllAsync()
        {
            var products = _memoryCache.Get<IEnumerable<Product>>(CacheProductKey);
            return Task.FromResult(products);
        }

        public Task<Product> GetByIdAsync(int id)
        {
            var product = _memoryCache.Get<List<Product>>(CacheProductKey).FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                throw new NotFoundException($"{typeof(Product).Name}({id}) not found.");
            }

            return Task.FromResult(product);
        }

        public Task<CustomResponseDto<List<ProductWithCategoryDto>>> GetProductsWithCategory()
        {
           var products = _memoryCache.Get<IEnumerable<Product>>(CacheProductKey);

            var producstWithCategoryDto = _mapper.Map<List<ProductWithCategoryDto>>(products);

            return Task.FromResult(CustomResponseDto<List<ProductWithCategoryDto>>.Success(200, producstWithCategoryDto));
        }

        public async Task removeAsync(Product entity)
        {
            _repository.remove(entity);
            await _unitOfWork.CommitAsync();
            await CacheAllProductsAsync();  
        }

        public async Task RemoveRangeAsync(IEnumerable<Product> entities)
        {
            _repository.RemoveRange(entities);
            await _unitOfWork.CommitAsync();
            await CacheAllProductsAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
            _repository.Update(entity);
            await _unitOfWork.CommitAsync();
            await CacheAllProductsAsync();
        }

        public IQueryable<Product> Where(Expression<Func<Product, bool>> exp)
        {
            return _memoryCache.Get<List<Product>>(CacheProductKey).Where(exp.Compile()).AsQueryable();
        }

        public async Task CacheAllProductsAsync() //cok sık degistirmeyecegimiz fakat cok sık erisecegimiz data en uygun cache adayidir
        {
           _memoryCache.Set(CacheProductKey, await _repository.GetAll().ToListAsync());
        }
    }
}
