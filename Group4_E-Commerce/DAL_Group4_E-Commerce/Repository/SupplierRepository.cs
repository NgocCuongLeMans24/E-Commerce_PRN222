using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Group4_E_Commerce.Repository
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly EcommercePrn222Context _context;

        public SupplierRepository(EcommercePrn222Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .OrderBy(s => s.CompanyName)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(string supplierId)
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        }

        public async Task<Supplier?> GetByEmailAsync(string email)
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> CreateAsync(Supplier supplier)
        {
            try
            {
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            try
            {
                _context.Suppliers.Update(supplier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string supplierId)
        {
            try
            {
                var supplier = await GetByIdAsync(supplierId);
                if (supplier != null)
                {
                    _context.Suppliers.Remove(supplier);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string supplierId)
        {
            return await _context.Suppliers.AnyAsync(s => s.SupplierId == supplierId);
        }

        public async Task<bool> EmailExistsAsync(string email, string? excludeSupplierId = null)
        {
            var query = _context.Suppliers.Where(s => s.Email.ToLower() == email.ToLower());

            if (!string.IsNullOrEmpty(excludeSupplierId))
            {
                query = query.Where(s => s.SupplierId != excludeSupplierId);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Supplier>> SearchAsync(string searchTerm)
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .Where(s => s.CompanyName.Contains(searchTerm) ||
                           s.Email.Contains(searchTerm) ||
                           (s.ContactName != null && s.ContactName.Contains(searchTerm)))
                .OrderBy(s => s.CompanyName)
                .ToListAsync();
        }
    }
}
