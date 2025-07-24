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
                Categories = _productService.GetCategories()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var product = new Product
                {
                    ProductName = model.ProductName,
                    Description = model.Description,
                    UnitPrice = (double?)model.Price,
                    SupplierId = supplierId,
                    CategoryId = (int)model.CategoryId
                };

                _productService.Add(product);
                TempData["Success"] = "Product created.";
                return RedirectToAction("MyProducts");
            }

            model.Categories = _productService.GetCategories();
            return View(model);
        }




        [HttpGet]
        public IActionResult Edit(int id)
        {
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var product = _productService.GetById(id);
            if (product == null || product.SupplierId != supplierId)
                return Forbid();

            var model = new ProductManagementViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = (decimal)(product.UnitPrice ?? 0),
                CategoryId = product.CategoryId

            };


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductManagementViewModel model)
        {
            var product = _productService.GetById(model.ProductId);
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product == null || product.SupplierId != supplierId)
                return Forbid();

            // cập nhật thông tin
            product.ProductName = model.ProductName;
            //...
            _productService.Update(product);

            return RedirectToAction("MyProducts");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var product = _productService.GetById(id);
            var supplierId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (product == null || product.SupplierId != supplierId)
                return Forbid();

            _productService.Delete(id);
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
