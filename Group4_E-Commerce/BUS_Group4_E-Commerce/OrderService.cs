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
        private readonly IOrderRepository _orderRepo;

        public OrderService(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public IEnumerable<Order> GetOrdersByCustomerId(string customerId)
        {
            return _orderRepo.GetOrdersByCustomerId(customerId);
        }

        public Order? GetOrderWithDetails(int orderId, string customerId)
        {
            return _orderRepo.GetOrderWithDetails(orderId, customerId);
        }

		    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
		    {
			  return await _orderRepo.GetAllAsync();
		    }

		    public async Task<IEnumerable<Order>> SearchOrdersAsync(string? searchTerm)
		    {
			  return await _orderRepo.SearchAsync(searchTerm);
		    } 

        public List<OrderDetail> GetOrderDetailsBySupplierId(string supplierId)
        {
            return _orderRepo.GetOrderDetailsBySupplierId(supplierId);
        }
    }
}

