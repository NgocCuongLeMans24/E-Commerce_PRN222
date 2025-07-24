using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ViewModels;
using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels;
using Group4MVC.Attributes;

namespace Controllers
{
    [Authorize]
    [RequirePermission("Analytics")]
    public class AnalyticsController : Controller
    {
        private readonly EcommercePrn222Context _context;

        public AnalyticsController(EcommercePrn222Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new SalesAnalyticsViewModel();

            // Get current date for calculations
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            // Calculate total sales from OrderDetails
            var totalSales = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3) // Completed orders
                .SumAsync(od => (od.Quantity) * (od.UnitPrice));

            // Calculate monthly sales
            var monthlySales = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate >= startOfMonth)
                .SumAsync(od => (od.Quantity) * (od.UnitPrice));

            // Calculate weekly sales
            var weeklySales = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate >= startOfWeek)
                .SumAsync(od => (od.Quantity) * (od.UnitPrice));

            // Calculate daily sales
            var dailySales = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate.HasValue && od.Order.OrderDate.Value.Date == today)
                .SumAsync(od => (od.Quantity) * (od.UnitPrice));

            // Get total orders count
            var totalOrders = await _context.Orders
                .Where(o => o.StatusId == 3)
                .CountAsync();

            // Get monthly orders count
            var monthlyOrders = await _context.Orders
                .Where(o => o.StatusId == 3 && o.OrderDate >= startOfMonth)
                .CountAsync();

            // Calculate average order value
            var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

            // Get sales trend for last 30 days
            var salesTrend = new List<SalesTrendData>();
            for (int i = 29; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dailyTotal = await _context.OrderDetails
                    .Include(od => od.Order)
                    .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate.HasValue && od.Order.OrderDate.Value.Date == date)
                    .SumAsync(od => (od.Quantity) * (od.UnitPrice));

                salesTrend.Add(new SalesTrendData
                {
                    Date = date,
                    Amount = (decimal)dailyTotal
                });
            }

            // Get top selling products
            var topProducts = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3)
                .GroupBy(od => new { od.ProductId, od.Product.ProductName })
                .Select(g => new TopProductData
                {
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = (decimal)g.Sum(od => (od.Quantity) * (od.UnitPrice))
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(10)
                .ToListAsync();

            // Get monthly sales comparison (current vs previous month)
            var previousMonth = startOfMonth.AddMonths(-1);
            var previousMonthEnd = startOfMonth.AddDays(-1);
            var previousMonthSales = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate >= previousMonth && od.Order.OrderDate <= previousMonthEnd)
                .SumAsync(od => (od.Quantity) * (od.UnitPrice));

            var monthlyGrowth = previousMonthSales > 0
                ? ((monthlySales - previousMonthSales) / previousMonthSales) * 100
                : 0;

            viewModel.TotalSales = (decimal)totalSales;
            viewModel.MonthlySales = (decimal)monthlySales;
            viewModel.WeeklySales = (decimal)weeklySales;
            viewModel.DailySales = (decimal)dailySales;
            viewModel.TotalOrders = totalOrders;
            viewModel.MonthlyOrders = monthlyOrders;
            viewModel.AverageOrderValue = (decimal)averageOrderValue;
            viewModel.MonthlyGrowthPercentage = (decimal)monthlyGrowth;
            viewModel.SalesTrend = salesTrend;
            viewModel.TopProducts = topProducts;

            return View(viewModel);
        }

        public async Task<IActionResult> Sales()
        {
            var viewModel = new DetailedSalesAnalyticsViewModel();

            // Get sales data for different periods
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            // Monthly sales for current year
            var monthlySalesData = new List<MonthlySalesData>();
            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(today.Year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthSales = await _context.OrderDetails
                    .Include(od => od.Order)
                    .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate >= monthStart && od.Order.OrderDate <= monthEnd)
                    .SumAsync(od => (od.Quantity) * (od.UnitPrice));

                var monthOrders = await _context.Orders
                    .Where(o => o.StatusId == 3 && o.OrderDate >= monthStart && o.OrderDate <= monthEnd)
                    .CountAsync();

                monthlySalesData.Add(new MonthlySalesData
                {
                    Month = monthStart.ToString("MMM"),
                    Sales = (decimal)monthSales,
                    Orders = monthOrders
                });
            }

            // Sales by category
            var salesByCategory = await _context.OrderDetails
                .Include(od => od.Product)
                .ThenInclude(p => p.Category)
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3)
                .GroupBy(od => od.Product.Category.CategoryName)
                .Select(g => new CategorySalesData
                {
                    CategoryName = g.Key,
                    TotalSales = (decimal)g.Sum(od => (od.Quantity) * (od.UnitPrice)),
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(c => c.TotalSales)
                .ToListAsync();

            // Recent orders with calculated totals
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .Include(o => o.OrderDetails)
                .Where(o => o.StatusId == 3)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrderData
                {
                    OrderId = o.OrderId,
                    CustomerName = o.Customer.FullName,
                    OrderDate = o.OrderDate ?? DateTime.Now,
                    TotalAmount = (decimal)o.OrderDetails.Sum(od => (od.Quantity) * (od.UnitPrice)),
                    Status = o.Status.StatusName
                })
                .ToListAsync();

            viewModel.MonthlySalesData = monthlySalesData;
            viewModel.SalesByCategory = salesByCategory;
            viewModel.RecentOrders = recentOrders;

            // Calculate totals
            viewModel.YearToDateSales = (decimal)await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate >= startOfYear)
                .SumAsync(od => (od.Quantity) * (od.UnitPrice));

            viewModel.YearToDateOrders = await _context.Orders
                .Where(o => o.StatusId == 3 && o.OrderDate >= startOfYear)
                .CountAsync();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesData(string period = "daily", int days = 30)
        {
            var today = DateTime.Today;
            var salesData = new List<object>();

            switch (period.ToLower())
            {
                case "daily":
                    for (int i = days - 1; i >= 0; i--)
                    {
                        var date = today.AddDays(-i);
                        var dailyTotal = await _context.OrderDetails
                            .Include(od => od.Order)
                            .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate.HasValue && od.Order.OrderDate.Value.Date == date)
                            .SumAsync(od => (od.Quantity) * (od.UnitPrice));

                        salesData.Add(new
                        {
                            date = date.ToString("yyyy-MM-dd"),
                            amount = dailyTotal
                        });
                    }
                    break;

                case "weekly":
                    for (int i = 11; i >= 0; i--)
                    {
                        var weekStart = today.AddDays(-((int)today.DayOfWeek + (i * 7)));
                        var weekEnd = weekStart.AddDays(6);

                        var weeklyTotal = await _context.OrderDetails
                            .Include(od => od.Order)
                            .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate.HasValue &&
                                        od.Order.OrderDate.Value.Date >= weekStart && od.Order.OrderDate.Value.Date <= weekEnd)
                            .SumAsync(od => (od.Quantity) * (od.UnitPrice));

                        salesData.Add(new
                        {
                            date = $"Week of {weekStart:MMM dd}",
                            amount = weeklyTotal
                        });
                    }
                    break;

                case "monthly":
                    for (int i = 11; i >= 0; i--)
                    {
                        var monthStart = today.AddMonths(-i);
                        monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        var monthlyTotal = await _context.OrderDetails
                            .Include(od => od.Order)
                            .Where(od => od.Order.StatusId == 3 && od.Order.OrderDate >= monthStart && od.Order.OrderDate <= monthEnd)
                            .SumAsync(od => (od.Quantity) * (od.UnitPrice));

                        salesData.Add(new
                        {
                            date = monthStart.ToString("MMM yyyy"),
                            amount = monthlyTotal
                        });
                    }
                    break;
            }

            return Json(salesData);
        }
    }
}
