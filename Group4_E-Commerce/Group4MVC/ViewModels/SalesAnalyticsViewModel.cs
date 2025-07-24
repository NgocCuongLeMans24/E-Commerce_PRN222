namespace ViewModels
{
    public class SalesAnalyticsViewModel
    {
        public decimal TotalSales { get; set; }
        public decimal MonthlySales { get; set; }
        public decimal WeeklySales { get; set; }
        public decimal DailySales { get; set; }
        public int TotalOrders { get; set; }
        public int MonthlyOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal MonthlyGrowthPercentage { get; set; }
        public List<SalesTrendData> SalesTrend { get; set; } = new List<SalesTrendData>();
        public List<TopProductData> TopProducts { get; set; } = new List<TopProductData>();
    }

    public class DetailedSalesAnalyticsViewModel
    {
        public decimal YearToDateSales { get; set; }
        public int YearToDateOrders { get; set; }
        public List<MonthlySalesData> MonthlySalesData { get; set; } = new List<MonthlySalesData>();
        public List<CategorySalesData> SalesByCategory { get; set; } = new List<CategorySalesData>();
        public List<RecentOrderData> RecentOrders { get; set; } = new List<RecentOrderData>();
    }

    public class SalesTrendData
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class TopProductData
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MonthlySalesData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Sales { get; set; }
        public int Orders { get; set; }
    }

    public class CategorySalesData
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class RecentOrderData
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
