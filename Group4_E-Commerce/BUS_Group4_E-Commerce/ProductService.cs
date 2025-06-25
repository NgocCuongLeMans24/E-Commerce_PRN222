using DAL_Group4_E_Commerce.Models;
using DAL_Group4_E_Commerce.Repository;
using System.Collections.Generic;

namespace BUS_Group4_E_Commerce
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(EcommercePrn222Context context)
        {
            _productRepository = new ProductRepository(context);
        }

        public List<Product> GetAll()
        {
            return _productRepository.GetAll().ToList();
        }

        public Product? GetById(int id)
        {
            return _productRepository.GetById(id);
        }

        public List<Product> Filter(int? categoryId = null, string? supplierId = null, string? keyword = null)
        {
            return _productRepository.Filter(categoryId, supplierId, keyword).ToList();
        }

        public List<Product> GetPaged(int page, int pageSize, out int total, int? categoryId = null, string? supplierId = null, string? keyword = null)
        {
            return _productRepository.GetPaged(page, pageSize, out total, categoryId, supplierId, keyword);
        }

        public List<Product> GetRelatedProducts(int productId, int take = 4)
        {
            return _productRepository.GetRelatedProducts(productId, take);
        }

        public List<Product> GetBySupplier(string supplierId, int take = 4)
        {
            return _productRepository.GetBySupplier(supplierId, take);
        }
    }
}
