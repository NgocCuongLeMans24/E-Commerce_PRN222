﻿using DAL_Group4_E_Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUS_Group4_E_Commerce
{
    public interface IOrderService
    {
        IEnumerable<Order> GetOrdersByCustomerId(string customerId);
        Order? GetOrderWithDetails(int orderId, string customerId);

		Task<IEnumerable<Order>> GetAllOrdersAsync();

		Task<IEnumerable<Order>> SearchOrdersAsync(string? searchTerm);
        Task<Order?> GetOrderByIdAsync(int id);
        Task UpdateOrderAsync(Order order);

        List<OrderDetail> GetOrderDetailsBySupplierId(string supplierId);
    }

}
