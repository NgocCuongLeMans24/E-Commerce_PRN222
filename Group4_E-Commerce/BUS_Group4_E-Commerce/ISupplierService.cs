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



    }

}
