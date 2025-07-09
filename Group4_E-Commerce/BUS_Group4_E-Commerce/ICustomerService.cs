using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface ICustomerService
    {
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<Customer?> GetCustomerByIdAsync(string customerId);
        Task<bool> RegisterCustomerAsync(Customer customer);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> ActivateCustomerAsync(string customerId);
        Task<bool> DeactivateCustomerAsync(string customerId);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
    }
}
