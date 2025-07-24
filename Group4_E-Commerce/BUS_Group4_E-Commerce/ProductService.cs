using DAL_Group4_E_Commerce.Models;
using DAL_Group4_E_Commerce.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BUS_Group4_E_Commerce
{
    public class ProductService : IProductService
    {
        private readonly EcommercePrn222Context _context;
        private readonly IProductRepository _productRepository;
        public ProductService(EcommercePrn222Context context)
        {
            _context = context;
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

        public List<Category> GetCategories()
        {
            return _productRepository.GetCategories();
        }

        public List<Supplier> GetSuppliers()
        {
            return _productRepository.GetSuppliers();
        }

        public void Add(Product product)
        {
            _productRepository.Add(product);
        }

        public void Update(Product product)
        {
            _productRepository.Update(product);
        }

        public void Delete(int productId)
        {
            var product = _productRepository.GetById(productId);
            if (product != null)
            {
                _productRepository.Delete(product); 
            }
        }

        public List<Product> GetPaged(int page, int pageSize, out int total,
    int? categoryId = null, string? supplierId = null, string? keyword = null,
    string? sortBy = "ProductId", string? sortOrder = "asc")
        {
            return _productRepository.GetPaged(page, pageSize, out total, categoryId, supplierId, keyword, sortBy, sortOrder);
        }

        public IEnumerable<OrderDetail> GetOrderHistoryBySupplier(string supplierId)
        {
            return _context.OrderDetails
                .Where(od => od.Product.SupplierId == supplierId)
                .Include(od => od.Order)
                .Include(od => od.Product)
                .OrderByDescending(od => od.Order.OrderDate)
                .ToList();
        }


    }
}
