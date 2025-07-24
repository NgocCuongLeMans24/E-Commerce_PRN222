using DAL_Group4_E_Commerce.Models;
using System.Collections.Generic;

namespace BUS_Group4_E_Commerce
{
    public interface IProductService
    {
        List<Product> GetAll();
        Product? GetById(int id);
        List<Product> Filter(int? categoryId = null, string? supplierId = null, string? keyword = null);
        List<Product> GetPaged(int page, int pageSize, out int total, int? categoryId = null, string? supplierId = null, string? keyword = null);
        List<Product> GetRelatedProducts(int productId, int take = 4);
        List<Product> GetBySupplier(string supplierId, int take = 4);
        List<Category> GetCategories();
        List<Supplier> GetSuppliers();

        void Add(Product product);
        void Update(Product product);

        void Delete(int productId);

        List<Product> GetPaged(int page, int pageSize, out int total,
    int? categoryId = null, string? supplierId = null, string? keyword = null,
    string? sortBy = "ProductId", string? sortOrder = "asc");

        public IEnumerable<OrderDetail> GetOrderHistoryBySupplier(string supplierId);

    }
}
