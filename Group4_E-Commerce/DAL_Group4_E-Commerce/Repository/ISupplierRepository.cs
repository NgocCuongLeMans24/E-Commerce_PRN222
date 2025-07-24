using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Group4_E_Commerce.Repository
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(string supplierId);
        Task<Supplier?> GetByEmailAsync(string email);
        Task<bool> CreateAsync(Supplier supplier);
        Task<bool> UpdateAsync(Supplier supplier);
        Task<bool> DeleteAsync(string supplierId);
        Task<bool> ExistsAsync(string supplierId);
        Task<bool> EmailExistsAsync(string email, string? excludeSupplierId = null);
        Task<IEnumerable<Supplier>> SearchAsync(string searchTerm);
    }
}
