using GUI_Group4_ECommerce.Helpers;
using GUI_Group4_ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GUI_Group4_ECommerce.ViewComponents
{
    public class CartViewComponent : ViewComponent
{
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new  List<CartItem>();
            return View("CartPanel", new CartModel
            {
                Quantity = cart.Sum(p => p.Quantity),
                Total = cart.Sum(p => p.Total)
            }
                );
        }
}
}
