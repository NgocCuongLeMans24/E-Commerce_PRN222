using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Group4_E_Commerce.Repository
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByEmailAsync(string email);
        Task<Employee?> GetByIdAsync(string employeeId);
        Task<Employee> CreateAsync(Employee employee);
        Task<Employee> UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(string employeeId);
        Task<bool> ExistsAsync(string employeeId);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<Employee>> GetAllAsync();
    }
}
