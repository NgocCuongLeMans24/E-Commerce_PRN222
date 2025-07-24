using DAL_Group4_E_Commerce.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
	public class ProductManagementViewModel
	{
		public List<Product> Products { get; set; } = new();
		public List<Category> Categories { get; set; } = new();
		public List<Supplier> Suppliers { get; set; } = new();

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public int? CategoryId { get; set; }
		public string? SupplierId { get; set; }
		public string? Keyword { get; set; }

		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int TotalCount { get; set; }

		public string SortBy { get; set; } = "ProductId";
		public string SortOrder { get; set; } = "asc";

		public int ProductId { get; set; }

		[Required]
		public string ProductName { get; set; }

		public string? Description { get; set; }

		[Range(0, double.MaxValue)]
		public decimal Price { get; set; }

        public IEnumerable<OrderDetail>? OrderHistory { get; set; }
    }
}
