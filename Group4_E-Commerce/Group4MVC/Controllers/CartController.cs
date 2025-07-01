using DAL_Group4_E_Commerce.Models;
using GUI_Group4_ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GUI_Group4_ECommerce.Helpers;

namespace GUI_Group4_ECommerce.Controllers
{
    public class CartController : Controller
{
        private readonly EcommercePrn222Context db;

        public CartController(EcommercePrn222Context context) {
            db = context;
        }

        const string CART_KEY = "MYCART";
        public List<CartItem> CartItems => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new
            List<CartItem>();
        public IActionResult Index()
        {
            return View(CartItems);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var cart = CartItems;
            var item = cart.SingleOrDefault(p => p.ProductId == id);
            if (item == null)
            {
                var product = db.Products.SingleOrDefault(p => p.ProductId == id);
                if (product == null) { 
                    return NotFound();
                }
                item = new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    UnitPrice = product.UnitPrice ?? 0,
                    Image = product.Image ?? String.Empty,
                    Quantity = quantity
                };
                cart.Add(item);
            }
            else
            {
                item.Quantity += quantity;
            }
            HttpContext.Session.Set(CART_KEY, cart);
                return RedirectToAction("index");
        }

        public IActionResult UpdateQuantity(int id, int delta)   
        {
            var cart = CartItems;
            var item = cart.SingleOrDefault(p => p.ProductId == id);
            if (item != null)
            {
                item.Quantity += delta;
                if (item.Quantity <= 0) cart.Remove(item);
                HttpContext.Session.Set(CART_KEY, cart);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int id)
        {
            var cart = CartItems;
            var item = cart.SingleOrDefault(p => p.ProductId == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(CART_KEY, cart);
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
