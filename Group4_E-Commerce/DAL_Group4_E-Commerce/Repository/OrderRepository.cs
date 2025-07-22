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

		public async Task<IEnumerable<Order>> GetAllAsync()
		{
			return await _context.Orders.ToListAsync();
		}

		public async Task<IEnumerable<Order>> SearchAsync(string? searchTerm)
		{
			var query = _context.Orders
				.Include(o => o.Customer)
				.Include(o => o.Employee)
				.Include(o => o.Status)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				query = query.Where(o =>
					o.OrderId.ToString().Contains(searchTerm) ||
					o.CustomerId.Contains(searchTerm) ||
					o.CustomerName.Contains(searchTerm) ||
					o.PhoneNumber.Contains(searchTerm) ||
					o.Status.StatusName.Contains(searchTerm)
				);
			}

			return await query.ToListAsync();
		}

	}

}
