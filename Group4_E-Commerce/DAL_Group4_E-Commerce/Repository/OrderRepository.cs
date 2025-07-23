using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL_Group4_E_Commerce.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EcommercePrn222Context _context;

        public OrderRepository(EcommercePrn222Context context)
        {   
            _context = context;
        }

        public IEnumerable<Order> GetOrdersByCustomerId(string customerId)
        {
            return _context.Orders
                .Include(o => o.Status)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetOrderWithDetails(int orderId, string customerId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Status)
                .FirstOrDefault(o => o.OrderId == orderId && o.CustomerId == customerId);
        }
    }
}
