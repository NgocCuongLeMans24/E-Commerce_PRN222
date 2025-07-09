using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface IEmployeeService
    {
        Task<Employee?> GetEmployeeByEmailAsync(string email);
        Task<Employee?> GetEmployeeByIdAsync(string employeeId);
        Task<bool> CreateEmployeeAsync(Employee employee, string password);
        Task<bool> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteAsync(string employeeId);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    }
}
