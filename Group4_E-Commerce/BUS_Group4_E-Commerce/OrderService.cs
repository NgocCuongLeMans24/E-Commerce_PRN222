using DAL_Group4_E_Commerce.Models;
using DAL_Group4_E_Commerce.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
	public class OrderService : IOrderService
	{
		private readonly IOrderRepository _orderRepository;

		public OrderService(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<IEnumerable<Order>> GetAllOrdersAsync()
		{
			return await _orderRepository.GetAllAsync();
		}

		public async Task<IEnumerable<Order>> SearchOrdersAsync(string? searchTerm)
		{
			return await _orderRepository.SearchAsync(searchTerm);
		}
	}
}
