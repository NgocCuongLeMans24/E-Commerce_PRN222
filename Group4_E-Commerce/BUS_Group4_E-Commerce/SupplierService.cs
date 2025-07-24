using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class SupplierService : ISupplierService
    {
        private readonly EcommercePrn222Context _context;

        public SupplierService(EcommercePrn222Context context)
        {
            _context = context;
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


    }

}
