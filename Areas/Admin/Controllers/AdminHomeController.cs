using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenNgocThien_BanHang.Data;
using NguyenNgocThien_Buoi4.Models;
using System.Linq;

namespace NguyenNgocThien_BanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            // Đơn hàng thống kê
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            ViewBag.ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == "Processing");
            ViewBag.DeliveredOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered");
            ViewBag.CancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled");

            // Tổng doanh thu
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status == "Delivered")
                .SumAsync(o => o.TotalAmount);

            // Doanh thu theo tháng (6 tháng gần đây)
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var monthlyRevenue = await _context.Orders
                .Where(o => o.Status != "Cancelled" && o.OrderDate >= sixMonthsAgo)
                .GroupBy(o => new { Month = o.OrderDate.Month, Year = o.OrderDate.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            ViewBag.MonthlyRevenue = monthlyRevenue;

            // Sản phẩm bán chạy
            var topProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            var productIds = topProducts.Select(p => p.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            ViewBag.TopProducts = topProducts.Select(tp => new
            {
                Product = products.FirstOrDefault(p => p.Id == tp.ProductId),
                TotalQuantity = tp.TotalQuantity
            }).ToList();

            // Đơn hàng gần đây
            var recentOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}