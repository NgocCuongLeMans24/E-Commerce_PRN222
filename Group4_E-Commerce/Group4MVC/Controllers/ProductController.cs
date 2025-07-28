using Microsoft.AspNetCore.Mvc;
using BUS_Group4_E_Commerce;
using DAL_Group4_E_Commerce.Models;
using System.Linq;

namespace Group4MVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(EcommercePrn222Context context)
        {
            _productService = new ProductService(context);
        }

        public IActionResult Index(int? categoryId, string? supplierId, string? keyword, int page = 1, int pageSize = 9)
        {
            int total;
            var products = _productService.GetPaged(page, pageSize, out total, categoryId, supplierId, keyword);
            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Categories = _productService.GetCategories();
            ViewBag.Suppliers = _productService.GetSuppliers();
            return View(products);
        }

        public IActionResult Detail(int id)
        {
            var product = _productService.GetById(id);
            if (product == null) return NotFound();
            ViewBag.Related = _productService.GetRelatedProducts(id);
            ViewBag.SameSupplier = _productService.GetBySupplier(product.SupplierId);
            return View(product);   
        }

        [HttpGet]
        public IActionResult AjaxList(int? categoryId, string? supplierId, string? keyword, int page = 1, int pageSize = 9)
        {
            int total;
            var products = _productService.GetPaged(page, pageSize, out total, categoryId, supplierId, keyword);
            ViewBag.Categories = _productService.GetCategories();
            ViewBag.Suppliers = _productService.GetSuppliers();
            return PartialView("_ProductListPartial", products);
        }

        [HttpGet]
        public IActionResult Compare(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return RedirectToAction("Index");
            var idList = ids.Split(',').Select(id => int.TryParse(id, out var i) ? i : (int?)null).Where(i => i.HasValue).Select(i => i.Value).ToList();
            if (!idList.Any()) return RedirectToAction("Index");
            
            var products = _productService.GetAll().Where(p => idList.Contains(p.ProductId)).ToList();
            
            // Check if all products have the same category
            if (products.Count > 1)
            {
                var firstCategoryId = products.First().CategoryId;
                if (products.Any(p => p.CategoryId != firstCategoryId))
                {
                    TempData["ErrorMessage"] = "You can only compare products from the same category!";
                    return RedirectToAction("Index");
                }
            }
            
            return View(products);
        }
    }
}
