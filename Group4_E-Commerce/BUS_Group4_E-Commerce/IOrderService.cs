using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
	public interface IOrderService
	{
		Task<IEnumerable<Order>> GetAllOrdersAsync();

		Task<IEnumerable<Order>> SearchOrdersAsync(string? searchTerm);

	}
}
