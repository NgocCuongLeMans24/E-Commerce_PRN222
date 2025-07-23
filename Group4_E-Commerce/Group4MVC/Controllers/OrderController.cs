using BUS_Group4_E_Commerce;
using GUI_Group4_ECommerce.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GUI_Group4_ECommerce.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult MyOrders()
        {
            var idClaim = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID);
            if (idClaim == null) return Unauthorized();

            var orders = _orderService.GetOrdersByCustomerId(idClaim.Value);
            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var idClaim = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID);
            if (idClaim == null) return Unauthorized();

            var order = _orderService.GetOrderWithDetails(id, idClaim.Value);
            if (order == null) return NotFound();

            return View(order);
        }
    }

}
