using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenNgocThien_BanHang.Data;
using NguyenNgocThien_BanHang.Models;
using NguyenNgocThien_BanHang.Services;
using NguyenNgocThien_Buoi4.Models;
using System.Diagnostics;

namespace NguyenNgocThien_BanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, CartService cartService)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy sản phẩm nổi bật
            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .Take(4)
                .ToListAsync();

            // Lấy danh mục
            var categories = await _context.Categories.ToListAsync();

            // Lấy số lượng sản phẩm trong giỏ hàng
            var cart = _cartService.GetCart();
            ViewBag.CartItemCount = cart.TotalItems;

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.Categories = categories;

            return View(featuredProducts);
        }

        public IActionResult Privacy()
        {
            // Lấy số lượng sản phẩm trong giỏ hàng
            var cart = _cartService.GetCart();
            ViewBag.CartItemCount = cart.TotalItems;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}