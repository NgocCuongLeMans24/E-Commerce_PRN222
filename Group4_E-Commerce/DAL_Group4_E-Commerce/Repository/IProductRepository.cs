using DAL_Group4_E_Commerce.Models;
using System.Collections.Generic;
using System.Linq;

namespace DAL_Group4_E_Commerce.Repository
{
    public interface IProductRepository
    {
        IQueryable<Product> GetAll();
        Product? GetById(int id);
        IQueryable<Product> Filter(int? categoryId = null, string? supplierId = null, string? keyword = null);
        List<Product> GetPaged(int page, int pageSize, out int total, int? categoryId = null, string? supplierId = null, string? keyword = null);
        List<Product> GetRelatedProducts(int productId, int take = 4);
        List<Product> GetBySupplier(string supplierId, int take = 4);
        List<Category> GetCategories();
        List<Supplier> GetSuppliers();
    }
}
