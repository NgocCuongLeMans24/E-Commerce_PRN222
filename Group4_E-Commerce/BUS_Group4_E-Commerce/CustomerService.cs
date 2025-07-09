using DAL_Group4_E_Commerce.Models;
using DAL_Group4_E_Commerce.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHashingService _passwordHashingService;

        public CustomerService(ICustomerRepository customerRepository, IPasswordHashingService passwordHashingService)
        {
            _customerRepository = customerRepository;
            _passwordHashingService = passwordHashingService;
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _customerRepository.GetByEmailAsync(email);
        }

        public async Task<Customer?> GetCustomerByIdAsync(string customerId)
        {
            return await _customerRepository.GetByIdAsync(customerId);
        }

        public async Task<bool> RegisterCustomerAsync(Customer customer)
        {
            try
            {
                if (await _customerRepository.EmailExistsAsync(customer.Email))
                {
                    return false;
                }

                customer.Password = _passwordHashingService.HashPassword(customer.Password ?? string.Empty);
                customer.RandomKey = Guid.NewGuid().ToString();
                customer.IsActive = true;
                customer.Role = 0;

                await _customerRepository.CreateAsync(customer);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                await _customerRepository.UpdateAsync(customer);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActivateCustomerAsync(string customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.IsActive = true;
                await _customerRepository.UpdateAsync(customer);
                return true;
            }
            return false;
        }

        public async Task<bool> DeactivateCustomerAsync(string customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer != null)
            {
                customer.IsActive = false;
                await _customerRepository.UpdateAsync(customer);
                return true;
            }
            return false;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _customerRepository.EmailExistsAsync(email);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _customerRepository.GetActiveCustomersAsync();
        }
    }
}
