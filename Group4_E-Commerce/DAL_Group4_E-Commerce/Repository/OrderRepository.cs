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

		/* -------------------- LẤY TẤT CẢ -------------------- */
		public async Task<IEnumerable<Order>> GetAllAsync()
		{
			return await _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.Employee)
				.Include(o => o.Status)
				.ToListAsync();
		}

		/* -------------------- SEARCH -------------------- */
		public async Task<IEnumerable<Order>> SearchAsync(string? searchTerm)
		{
			var query = _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.Employee)
				.Include(o => o.Status)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				var kw = searchTerm.ToLower();   // 👉 không phân biệt hoa‑thường
				query = query.Where(o =>
					o.OrderId.ToString().Contains(kw) ||
					(!string.IsNullOrEmpty(o.CustomerId) && o.CustomerId.ToLower().Contains(kw)) ||
					(o.Customer != null && !string.IsNullOrEmpty(o.Customer.FullName) && o.Customer.FullName.ToLower().Contains(kw)) ||
					(o.Customer != null && !string.IsNullOrEmpty(o.Customer.Email) && o.Customer.Email.ToLower().Contains(kw)) ||
					(!string.IsNullOrEmpty(o.Phone) && o.Phone.ToLower().Contains(kw)) ||
					(o.Status != null && !string.IsNullOrEmpty(o.Status.StatusName) && o.Status.StatusName.ToLower().Contains(kw))
				);
			}

			return await query.ToListAsync();
		}


        public List<OrderDetail> GetOrderDetailsBySupplierId(string supplierId)
        {
            return _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Product.SupplierId == supplierId)
                .OrderByDescending(od => od.Order.OrderDate)
                .ToList();
        }

    }
}
