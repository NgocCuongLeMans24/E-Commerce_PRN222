using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface IAuthenticationService
    {
        Task<Customer?> AuthenticateCustomerAsync(string email, string password);
        Task<Employee?> AuthenticateEmployeeAsync(string email, string password);
        Task<bool> RegisterCustomerAsync(Customer customer);
    }
}
