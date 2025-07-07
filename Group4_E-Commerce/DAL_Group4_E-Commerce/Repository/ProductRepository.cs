using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL_Group4_E_Commerce.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommercePrn222Context _context;
        public ProductRepository(EcommercePrn222Context context)
        {
            _context = context;
        }

        public IQueryable<Product> GetAll()
        {
            return _context.Products.Include(p => p.Category).Include(p => p.Supplier);
        }

        public Product? GetById(int id)
        {
            return _context.Products.Include(p => p.Category).Include(p => p.Supplier)
                .FirstOrDefault(p => p.ProductId == id);
        }

        public IQueryable<Product> Filter(int? categoryId = null, string? supplierId = null, string? keyword = null)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.Supplier).AsQueryable();
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);
            if (!string.IsNullOrEmpty(supplierId))
                query = query.Where(p => p.SupplierId == supplierId);
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.ProductName.Contains(keyword));
            return query;
        }

        public List<Product> GetPaged(int page, int pageSize, out int total, int? categoryId = null, string? supplierId = null, string? keyword = null)
        {
            var query = Filter(categoryId, supplierId, keyword);
            total = query.Count();
            return query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<Product> GetRelatedProducts(int productId, int take = 4)
        {
            var product = GetById(productId);
            if (product == null) return new List<Product>();
            return _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != productId)
                .Take(take)
                .ToList();
        }

        public List<Product> GetBySupplier(string supplierId, int take = 4)
        {
            return _context.Products
                .Where(p => p.SupplierId == supplierId)
                .Take(take)
                .ToList();
        }

        public List<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public List<Supplier> GetSuppliers()
        {
            return _context.Suppliers.ToList();
        }
    }
}
