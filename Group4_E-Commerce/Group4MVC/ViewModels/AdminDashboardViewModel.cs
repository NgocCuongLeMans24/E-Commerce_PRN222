using DAL_Group4_E_Commerce.Models;

namespace ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public List<Customer> RecentCustomers { get; set; } = new List<Customer>();
        public List<Employee> RecentEmployees { get; set; } = new List<Employee>();
    }
}
