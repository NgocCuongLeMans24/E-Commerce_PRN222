using DAL_Group4_E_Commerce.Models;
using System.Collections.Generic;

namespace ViewModels
{
	public class ProductManagementViewModel
	{
		public List<Product> Products { get; set; } = new();
		public List<Category> Categories { get; set; } = new();
		public List<Supplier> Suppliers { get; set; } = new();

		public int? CategoryId { get; set; }
		public string? SupplierId { get; set; }
		public string? Keyword { get; set; }

		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int TotalCount { get; set; }

		public string SortBy { get; set; } = "ProductId";
		public string SortOrder { get; set; } = "asc";
	}
}
