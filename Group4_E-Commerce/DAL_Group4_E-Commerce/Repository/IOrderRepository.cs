using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Group4_E_Commerce.Repository
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOrdersByCustomerId(string customerId);
        Order? GetOrderWithDetails(int orderId, string customerId);

		Task<IEnumerable<Order>> GetAllAsync();

		Task<IEnumerable<Order>> SearchAsync(string? searchTerm);
        Task<Order?> GetByIdAsync(int id);
        Task<Order> UpdateAsync(Order order);
        List<OrderDetail> GetOrderDetailsBySupplierId(string supplierId);


    }
}
