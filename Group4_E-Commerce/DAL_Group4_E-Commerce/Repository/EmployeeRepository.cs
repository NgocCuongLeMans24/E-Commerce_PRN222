using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Group4_E_Commerce.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EcommercePrn222Context _context;

        public EmployeeRepository(EcommercePrn222Context context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<Employee?> GetByIdAsync(string employeeId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<Employee> CreateAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> DeleteAsync(string employeeId)
        {
            var employee = await GetByIdAsync(employeeId);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ExistsAsync(string employeeId)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Employees
                .AnyAsync(e => e.Email == email);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.ToListAsync();
        }
    }
}
