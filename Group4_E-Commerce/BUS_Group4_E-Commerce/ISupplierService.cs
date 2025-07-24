using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface ISupplierService
    {
        Task AuthenticateSupplierAsync(string email, object password);
        Task<Supplier?> GetSupplierByEmailAsync(string email);
        Supplier GetSupplierById(string id);
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(string supplierId);
        Task<bool> CreateSupplierAsync(Supplier supplier);
        Task<bool> UpdateSupplierAsync(Supplier supplier);
        Task<bool> DeleteSupplierAsync(string supplierId);
        Task<bool> SupplierExistsAsync(string supplierId);
        Task<bool> EmailExistsAsync(string email, string? excludeSupplierId = null);
        Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm);
        Task<bool> ValidateSupplierCredentialsAsync(string email, string password);
        Task<bool> UpdateSupplierPasswordAsync(string supplierId, string newPassword);

    }

}
