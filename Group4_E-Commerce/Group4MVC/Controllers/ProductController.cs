using Microsoft.AspNetCore.Mvc;
using BUS_Group4_E_Commerce;
using DAL_Group4_E_Commerce.Models;

namespace Group4MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(EcommercePrn222Context context)
        {
            _productService = new ProductService(context);
        }

        // Danh sách s?n ph?m v?i l?c, phân trang
        public IActionResult Index(int? categoryId, string? supplierId, string? keyword, int page = 1, int pageSize = 9)
        {
            int total;
            var products = _productService.GetPaged(page, pageSize, out total, categoryId, supplierId, keyword);
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            return View(products);
        }

        // Chi ti?t s?n ph?m
        public IActionResult Detail(int id)
        {
            var product = _productService.GetById(id);
            if (product == null) return NotFound();
            ViewBag.Related = _productService.GetRelatedProducts(id);
            ViewBag.SameSupplier = _productService.GetBySupplier(product.SupplierId);
            // Hàng ?ã xem có th? l?u vào session ho?c cookie
            return View(product);
        }

        // AJAX: L?c, phân trang
        [HttpGet]
        public IActionResult AjaxList(int? categoryId, string? supplierId, string? keyword, int page = 1, int pageSize = 9)
        {
            int total;
            var products = _productService.GetPaged(page, pageSize, out total, categoryId, supplierId, keyword);
            return PartialView("_ProductListPartial", products);
        }
    }
}
