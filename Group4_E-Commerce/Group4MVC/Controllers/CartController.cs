using DAL_Group4_E_Commerce.Models;
using GUI_Group4_ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GUI_Group4_ECommerce.Helpers;
using Microsoft.AspNetCore.Authorization;
using GUI_Group4_ECommerce.Services;
using Root_Namespace.Pages;

namespace GUI_Group4_ECommerce.Controllers
{
    public class CartController : Controller
{
        private readonly EcommercePrn222Context db;
        private readonly IVnPayService _vnPayService;

        public CartController(EcommercePrn222Context context, IVnPayService vnPayService) {
            db = context;
            _vnPayService = vnPayService;
        }

        public List<CartItem> CartItems => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new
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
            HttpContext.Session.Set(MySetting.CART_KEY, cart);
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
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
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
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }
            return RedirectToAction(nameof(Index));
        }
        [Authorize]

        [HttpGet]
        public IActionResult Checkout()
        {
            {
                if (CartItems.Count == 0)
                {
                    return Redirect("/");
                }
                return View(CartItems);
            }
        }


        [Authorize]

        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            if (payment == "Pay with VNPay")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = CartItems.Sum(p => p.Total),
                    CreatedDate = DateTime.Now,
                    Description = $"{model.FullName} {model.Phone}",
                    FullName = model.FullName,
                    OrderId = new Random().Next(1000,10000)
                };
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            // login check disabled for testing
            var idClaim = HttpContext.User.FindFirst(MySetting.CLAIM_CUSTOMERID);
            if (idClaim == null) return Unauthorized();
            string customerId = idClaim.Value;
          
            //string customerId = "1";

            var customer = db.Customers.SingleOrDefault(c => c.CustomerId == customerId);
            if (customer == null) return NotFound();
            if (model.SameAsCustomer)
            {
                model.FullName = customer.FullName;
                model.Phone = customer.Phone;
                model.Address = customer.Address;

                // optional: remove validation errors for these fields
                ModelState.Remove(nameof(model.FullName));
                ModelState.Remove(nameof(model.Phone));
                ModelState.Remove(nameof(model.Address));
            }

          
            if (!ModelState.IsValid) return View(CartItems);

            var order = new Order
            {
                CustomerId = customerId,
                CustomerName = model.FullName ?? customer.FullName,
                Address = model.Address ?? customer.Address,
                Phone = model.Phone ?? customer.Phone,
                OrderDate = DateTime.Now,
                PaymentMethod = payment,      
                ShippingMethod = "GRAB",
                StatusId = 0,
                Note = model.Note
            };

            db.Database.BeginTransaction();
            try
            {
                db.Add(order);
                db.SaveChanges();

                var details = CartItems.Select(i => new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = 0
                });
                db.AddRange(details);
                db.SaveChanges();
                db.Database.CommitTransaction();

                HttpContext.Session.Set(MySetting.CART_KEY, new List<CartItem>());
                return View("Success");
            }
            catch (Exception ex)
            {
                db.Database.RollbackTransaction();
                ModelState.AddModelError("", $"Order failed: {ex.Message}");
                return View(CartItems);
            }
        }

        [Authorize]


        public IActionResult PaymentFail()
        {
            return View();
        }


        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if(response == null || response.VnPayResponseCode != "00"){
                TempData["Message"] = $"VnPay payment error { response.VnPayResponseCode } ";
                return RedirectToAction("PaymentFail");
            }
            TempData["Message"] = $"VnPay payment success {response.VnPayResponseCode} ";
            return RedirectToAction("Success");
        }

        public IActionResult Success() => View("Success");
    }



    
}
