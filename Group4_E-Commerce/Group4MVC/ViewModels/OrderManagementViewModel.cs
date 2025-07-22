using DAL_Group4_E_Commerce.Models;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class OrderManagementViewModel
    {
		// Danh sách đơn hàng để hiển thị
		public IEnumerable<Order> Orders { get; set; }

		// Tìm kiếm
		public string SearchQuery { get; set; } = string.Empty;

		// Sắp xếp
		public string SortOrder { get; set; } = "asc";

		// Phân trang
		public int CurrentPage { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int TotalItems { get; set; }
		public int TotalPages => (int)System.Math.Ceiling((double)TotalItems / PageSize);
		// Thống kê
		public int TotalOrders => Orders != null ? Orders.Count() : 0;
	}
}
