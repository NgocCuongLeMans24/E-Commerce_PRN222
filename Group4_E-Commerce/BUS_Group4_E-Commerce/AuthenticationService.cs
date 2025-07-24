using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ISupplierService _supplierService;
        private readonly IPasswordHashingService _passwordHashingService;

        public AuthenticationService(
            ICustomerService customerService,
            IEmployeeService employeeService,
            ISupplierService supplierService,
            IPasswordHashingService passwordHashingService)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _supplierService = supplierService;
            _passwordHashingService = passwordHashingService;
        }

        public async Task<Customer?> AuthenticateCustomerAsync(string email, string password)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);

            if (customer != null && (bool)customer.IsActive && !string.IsNullOrEmpty(customer.Password))
            {
                if (_passwordHashingService.VerifyPassword(password, customer.Password))
                {
                    return customer;
                }
            }

            return null;
        }

        public async Task<Employee?> AuthenticateEmployeeAsync(string email, string password)
        {
            var employee = await _employeeService.GetEmployeeByEmailAsync(email);

            if (employee != null && !string.IsNullOrEmpty(employee.Password))
            {
                if (_passwordHashingService.VerifyPassword(password, employee.Password))
                {
                    return employee;
                }
            }

            return null;
        }

        public async Task<bool> RegisterCustomerAsync(Customer customer)
        {
            return await _customerService.RegisterCustomerAsync(customer);
        }

        public async Task<Supplier?> AuthenticateSupplierAsync(string email, string password)
        {
            var supplier = await _supplierService.GetSupplierByEmailAsync(email);
            if (supplier != null)
            {
                // Implement password check logic (you may need to add Password field in DB or hardcode temporarily)
                return supplier;
            }
            return null;
        }
    }
}
