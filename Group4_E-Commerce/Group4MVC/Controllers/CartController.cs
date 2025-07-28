using DAL_Group4_E_Commerce.Models;
using GUI_Group4_ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GUI_Group4_ECommerce.Helpers;
using Microsoft.AspNetCore.Authorization;
using GUI_Group4_ECommerce.Services;
using System.Diagnostics;

namespace GUI_Group4_ECommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly EcommercePrn222Context db;
        private readonly IVnPayService _vnPayService;

        public CartController(EcommercePrn222Context context, IVnPayService vnPayService)
        {
            db = context;
            _vnPayService = vnPayService;
        }

        public List<CartItem> CartItems => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

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
                if (product == null) return NotFound();

                item = new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    UnitPrice = product.UnitPrice ?? 0,
                    Image = product.Image ?? string.Empty,
                    Quantity = quantity
                };
                cart.Add(item);
            }
            else
            {
                item.Quantity += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, cart);
            return RedirectToAction(nameof(Index));
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
            if (CartItems.Count == 0)
            {
                return RedirectToAction("Index", "Product");
            }
            return View(CartItems);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            Console.WriteLine("Checkout called!");

            var idClaim = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID);
            foreach (var c in HttpContext.User.Claims)
            {
                Console.WriteLine($"Claim: {c.Type} = {c.Value}");
            }

            if (idClaim == null) return Unauthorized();
            string customerId = idClaim.Value;
            foreach (var claim in HttpContext.User.Claims)
            {
                Console.WriteLine($"CLAIM: {claim.Type} = {claim.Value}");
            }


            var customer = db.Customers.SingleOrDefault(c => c.CustomerId == customerId);
            if (customer == null) return NotFound();

            if (model.SameAsCustomer)
            {
                model.FullName = customer.FullName;
                model.Phone = customer.Phone;
                model.Address = customer.Address;

                ModelState.Remove(nameof(model.FullName));
                ModelState.Remove(nameof(model.Phone));
                ModelState.Remove(nameof(model.Address));
            }

            if (!ModelState.IsValid) return View(CartItems);

            var order = new Order
            {
                CustomerId = customerId,
                CustomerName = model.FullName,
                Phone = model.Phone,
                Address = model.Address,
                OrderDate = DateTime.Now,
                PaymentMethod = payment,
                ShippingMethod = "GRAB",
                StatusId = (payment == "Pay with VNPay") ? 2 : 1,
                Note = model.Note
            };

            db.Database.BeginTransaction();
            try
            {
                db.Add(order);
                db.SaveChanges(); // cần thiết để lấy được order.OrderId

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


                if (payment == "Pay with VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = CartItems.Sum(p => p.Total),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.FullName} {model.Phone}",
                        FullName = model.FullName,
                        OrderId = order.OrderId  // CHUẨN XÁC!
                    };

                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }

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
            if (response == null || response.VnPayResponseCode != "00")
            {
                if (int.TryParse(response?.OrderId, out int orderId))
                {
                    var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);

                    if (order != null)
                    {
                        order.StatusId = 1;
                        db.SaveChanges();
                    }
                }

                TempData["Message"] = $"VnPay payment error {response?.VnPayResponseCode}";
                return RedirectToAction(nameof(PaymentFail));
            }

            TempData["Message"] = $"VnPay payment success {response.VnPayResponseCode}";
            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View("Success");
        }

        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }
    }
}
