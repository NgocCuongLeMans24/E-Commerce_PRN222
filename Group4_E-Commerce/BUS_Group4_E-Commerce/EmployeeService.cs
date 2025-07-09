using DAL_Group4_E_Commerce.Models;
using DAL_Group4_E_Commerce.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPasswordHashingService _passwordHashingService;

        public EmployeeService(IEmployeeRepository employeeRepository, IPasswordHashingService passwordHashingService)
        {
            _employeeRepository = employeeRepository;
            _passwordHashingService = passwordHashingService;
        }

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            return await _employeeRepository.GetByEmailAsync(email);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
        {
            return await _employeeRepository.GetByIdAsync(employeeId);
        }

        public async Task<bool> CreateEmployeeAsync(Employee employee, string password)
        {
            try
            {
                if (await _employeeRepository.EmailExistsAsync(employee.Email))
                {
                    return false;
                }

                employee.Password = _passwordHashingService.HashPassword(password);
                await _employeeRepository.CreateAsync(employee);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            try
            {
                await _employeeRepository.UpdateAsync(employee);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string employeeId)
        {
            try
            {
                return await _employeeRepository.DeleteAsync(employeeId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _employeeRepository.EmailExistsAsync(email);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }
    }
}
