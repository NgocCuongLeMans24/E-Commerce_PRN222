using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DAL_Group4_E_Commerce.Repository;

namespace BUS_Group4_E_Commerce
{
    public class SupplierService : ISupplierService
    {
        private readonly EcommercePrn222Context _context;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPasswordHashingService _passwordHashingService;

        public SupplierService(EcommercePrn222Context context, ISupplierRepository supplierRepository, IPasswordHashingService passwordHashingService)
        {
            _context = context;
            _supplierRepository = supplierRepository;
            _passwordHashingService = passwordHashingService;
        }

        public Task AuthenticateSupplierAsync(string email, object password)
        {
            throw new NotImplementedException();
        }

        public async Task<Supplier?> GetSupplierByEmailAsync(string email)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.Email == email);
        }

        public Supplier GetSupplierById(string id)
        {
            return _context.Suppliers.FirstOrDefault(s => s.SupplierId == id);
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _supplierRepository.GetAllAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(string supplierId)
        {
            return await _supplierRepository.GetByIdAsync(supplierId);
        }

        public async Task<bool> CreateSupplierAsync(Supplier supplier)
        {
            try
            {
                // Check if supplier ID already exists
                if (await _supplierRepository.ExistsAsync(supplier.SupplierId))
                {
                    return false;
                }

                // Check if email already exists
                if (await _supplierRepository.EmailExistsAsync(supplier.Email))
                {
                    return false;
                }

                // Hash the password if provided
                if (!string.IsNullOrEmpty(supplier.Password))
                {
                    supplier.Password = _passwordHashingService.HashPassword(supplier.Password);
                }

                return await _supplierRepository.CreateAsync(supplier);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            try
            {
                // Check if email already exists for another supplier
                if (await _supplierRepository.EmailExistsAsync(supplier.Email, supplier.SupplierId))
                {
                    return false;
                }

                return await _supplierRepository.UpdateAsync(supplier);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSupplierAsync(string supplierId)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                if (supplier == null)
                {
                    return false;
                }

                // Check if supplier has products
                if (supplier.Products.Any())
                {
                    return false; // Cannot delete supplier with products
                }

                return await _supplierRepository.DeleteAsync(supplierId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SupplierExistsAsync(string supplierId)
        {
            return await _supplierRepository.ExistsAsync(supplierId);
        }

        public async Task<bool> EmailExistsAsync(string email, string? excludeSupplierId = null)
        {
            return await _supplierRepository.EmailExistsAsync(email, excludeSupplierId);
        }

        public async Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm)
        {
            return await _supplierRepository.SearchAsync(searchTerm);
        }

        public async Task<bool> ValidateSupplierCredentialsAsync(string email, string password)
        {
            try
            {
                var supplier = await _supplierRepository.GetByEmailAsync(email);
                if (supplier == null || string.IsNullOrEmpty(supplier.Password))
                {
                    return false;
                }

                return _passwordHashingService.VerifyPassword(password, supplier.Password);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSupplierPasswordAsync(string supplierId, string newPassword)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                if (supplier == null)
                {
                    return false;
                }

                supplier.Password = _passwordHashingService.HashPassword(newPassword);
                return await _supplierRepository.UpdateAsync(supplier);
            }
            catch
            {
                return false;
            }
        }
    }

}
