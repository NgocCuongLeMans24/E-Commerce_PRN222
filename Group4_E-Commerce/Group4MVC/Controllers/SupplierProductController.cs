using BUS_Group4_E_Commerce;
using DAL_Group4_E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ViewModels;

namespace Controllers
{
    [Authorize(Policy = "SupplierOnly")]
    public class SupplierProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        


        public SupplierProductController(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
            
        }


        public IActionResult MyProducts(
            string? keyword,
            int? categoryId,
            string? sortBy = "ProductId",
            string? sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var products = _productService.GetPaged(
                page, pageSize, out int total,
                categoryId, supplierId, keyword, sortBy, sortOrder);

            var orderDetails = _orderService.GetOrderDetailsBySupplierId(supplierId);

            var model = new ProductManagementViewModel
            {
                Products = products,
                Categories = _productService.GetCategories(), 
                Suppliers = _productService.GetSuppliers(),   
                Keyword = keyword,
                CategoryId = categoryId,
                SupplierId = supplierId,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = total,
                SortBy = sortBy,
                SortOrder = sortOrder,
                OrderDetails = orderDetails
            };


            return View(model);
        }



        public IActionResult Create()
        {
            var model = new ProductManagementViewModel
            {
                Categories = _productService.GetCategories(),
                OrderDetails = new List<OrderDetail>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = string.Join("; ", errors);
                model.Categories = _productService.GetCategories();
                model.OrderDetails = new List<OrderDetail>();
                return View(model);
            }

            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supplierId))
            {
                TempData["Error"] = "SupplierId is null. Please login as Supplier.";
                model.Categories = _productService.GetCategories();
                model.OrderDetails = new List<OrderDetail>();
                return View(model);
            }

            // Lấy các trường bổ sung từ form
            var alias = Request.Form["Alias"].ToString();
            var unitDescription = Request.Form["UnitDescription"].ToString();
            var discountStr = Request.Form["Discount"].ToString();
            var viewsStr = Request.Form["Views"].ToString();
            var manufactureDateStr = Request.Form["ManufactureDate"].ToString();
            var imageUrl = Request.Form["ImageUrl"].ToString();
            double? discount = null;
            int? views = null;
            DateTime? manufactureDate = null;
            if (double.TryParse(discountStr, out var d)) discount = d;
            if (int.TryParse(viewsStr, out var v)) views = v;
            if (DateTime.TryParse(manufactureDateStr, out var md)) manufactureDate = md;

            // Chỉ lưu URL ảnh, không upload cloudinary
            string imagePublicId = null;
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                imagePublicId = imageUrl.Trim();
            }

            var product = new Product
            {
                ProductName = model.ProductName,
                Alias = string.IsNullOrWhiteSpace(alias) ? null : alias,
                CategoryId = (int)model.CategoryId,
                UnitDescription = string.IsNullOrWhiteSpace(unitDescription) ? null : unitDescription,
                UnitPrice = (double?)model.Price,
                Image = imagePublicId,
                ManufactureDate = manufactureDate,
                Discount = discount,
                Views = views,
                Description = model.Description,
                SupplierId = supplierId
            };

            try
            {
                _productService.Add(product);
                TempData["Success"] = "Product created.";
                return RedirectToAction("MyProducts");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "DB Error: " + ex.Message;
                model.Categories = _productService.GetCategories();
                model.OrderDetails = new List<OrderDetail>();
                return View(model);
            }
        }




        [HttpGet]
        public IActionResult Edit(int id)
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = _productService.GetById(id);
            if (product == null || product.SupplierId != supplierId)
                return Forbid();

            var orderDetails = _orderService.GetOrderDetailsBySupplierId(supplierId);
            var model = new ProductManagementViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = (decimal)(product.UnitPrice ?? 0),
                CategoryId = product.CategoryId,
                Categories = _productService.GetCategories(),
                OrderDetails = orderDetails,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductManagementViewModel model)
        {
            var product = _productService.GetById(model.ProductId);
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product == null || product.SupplierId != supplierId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                model.Categories = _productService.GetCategories();
                model.OrderDetails = _orderService.GetOrderDetailsBySupplierId(supplierId);
                ViewBag.Product = product;
                return View(model);
            }

            // Lấy các trường bổ sung từ form
            var alias = Request.Form["Alias"].ToString();
            var unitDescription = Request.Form["UnitDescription"].ToString();
            var discountStr = Request.Form["Discount"].ToString();
            var viewsStr = Request.Form["Views"].ToString();
            var manufactureDateStr = Request.Form["ManufactureDate"].ToString();
            var imageUrl = Request.Form["ImageUrl"].ToString();
            double? discount = null;
            int? views = null;
            DateTime? manufactureDate = null;
            if (double.TryParse(discountStr, out var d)) discount = d;
            if (int.TryParse(viewsStr, out var v)) views = v;
            if (DateTime.TryParse(manufactureDateStr, out var md)) manufactureDate = md;

            // Chỉ lưu URL ảnh, không upload cloudinary
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                product.Image = imageUrl.Trim();
            }

            // Cập nhật các trường
            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.UnitPrice = (double?)model.Price;
            product.CategoryId = (int)model.CategoryId;
            product.Alias = string.IsNullOrWhiteSpace(alias) ? null : alias;
            product.UnitDescription = string.IsNullOrWhiteSpace(unitDescription) ? null : unitDescription;
            product.Discount = discount;
            product.Views = views;
            product.ManufactureDate = manufactureDate;
            // Image đã cập nhật ở trên nếu có nhập URL

            _productService.Update(product);

            return RedirectToAction("MyProducts");
        }

        [HttpPost]
        
        public IActionResult Delete(int id)
        {
            var product = _productService.GetById(id);
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product == null || product.SupplierId != supplierId)
            {
                TempData["Error"] = "Delete failed: Product not found or you do not have permission.";
                return RedirectToAction("MyProducts");
            }

            try
            {
                _productService.Delete(id);
                TempData["Success"] = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Delete failed: " + ex.Message;
            }
            return RedirectToAction("MyProducts");
        }

        public IActionResult OrderHistory()
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orderDetails = _productService.GetOrderHistoryBySupplier(supplierId);
            return View(orderDetails);
        }




    }

}
