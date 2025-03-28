//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using NguyenNgocThien_BanHang.Data;
//using NguyenNgocThien_BanHang.Services;
////using NguyenNgocThien_Buoi4.Data;
//using NguyenNgocThien_Buoi4.Models;
//using System.Linq;
//using System.Threading.Tasks;

//namespace NguyenNgocThien_Buoi4.Controllers
//{
//    public class StoreController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly CartService _cartService;

//        public StoreController(ApplicationDbContext context, CartService cartService)
//        {
//            _context = context;
//            _cartService = cartService;
//        }

//        // GET: Store
//        public async Task<IActionResult> Index(string searchString, int? categoryId, decimal? minPrice, decimal? maxPrice)
//        {
//            var products = _context.Products.Include(p => p.Category).AsQueryable();

//            // Tìm kiếm theo tên hoặc mô tả
//            if (!string.IsNullOrEmpty(searchString))
//            {
//                products = products.Where(p => p.Name.Contains(searchString) ||
//                                              p.Description.Contains(searchString));
//            }

//            // Lọc theo danh mục
//            if (categoryId.HasValue)
//            {
//                products = products.Where(p => p.CategoryId == categoryId.Value);
//            }

//            // Lọc theo giá
//            if (minPrice.HasValue)
//            {
//                products = products.Where(p => p.Price >= minPrice.Value);
//            }

//            if (maxPrice.HasValue)
//            {
//                products = products.Where(p => p.Price <= maxPrice.Value);
//            }

//            // Lấy danh sách danh mục cho bộ lọc
//            ViewBag.Categories = await _context.Categories.ToListAsync();
//            ViewBag.CategoryId = categoryId;
//            ViewBag.MinPrice = minPrice;
//            ViewBag.MaxPrice = maxPrice;
//            ViewBag.SearchString = searchString;

//            // Lấy số lượng sản phẩm trong giỏ hàng
//            ViewBag.CartItemCount = _cartService.GetTotalItems();

//            return View(await products.ToListAsync());
//        }

//        // GET: Store/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var product = await _context.Products
//                .Include(p => p.Category)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (product == null)
//            {
//                return NotFound();
//            }

//            // Lấy các sản phẩm liên quan (cùng danh mục)
//            var relatedProducts = await _context.Products
//                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
//                .Take(4)
//                .ToListAsync();

//            ViewBag.RelatedProducts = relatedProducts;
//            ViewBag.CartItemCount = _cartService.GetTotalItems();

//            return View(product);
//        }

//        // POST: Store/AddToCart/5
//        [HttpPost]
//        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
//        {
//            var product = await _context.Products.FindAsync(id);
//            if (product == null)
//            {
//                return NotFound();
//            }

//            if (product.Stock < quantity)
//            {
//                TempData["ErrorMessage"] = "Số lượng sản phẩm trong kho không đủ.";
//                return RedirectToAction(nameof(Details), new { id });
//            }

//            _cartService.AddToCart(product, quantity);
//            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng.";

//            return RedirectToAction(nameof(Cart));
//        }

//        // GET: Store/Cart
//        public IActionResult Cart()
//        {
//            var cart = _cartService.GetCart();
//            ViewBag.Total = _cartService.GetTotal();
//            return View(cart);
//        }

//        // POST: Store/UpdateCart
//        [HttpPost]
//        public IActionResult UpdateCart(int productId, int quantity)
//        {
//            _cartService.UpdateQuantity(productId, quantity);
//            return RedirectToAction(nameof(Cart));
//        }

//        // POST: Store/RemoveFromCart
//        [HttpPost]
//        public IActionResult RemoveFromCart(int productId)
//        {
//            _cartService.RemoveFromCart(productId);
//            return RedirectToAction(nameof(Cart));
//        }

//        // POST: Store/ClearCart
//        [HttpPost]
//        public IActionResult ClearCart()
//        {
//            _cartService.ClearCart();
//            return RedirectToAction(nameof(Cart));
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenNgocThien_BanHang.Data;
using NguyenNgocThien_BanHang.Services;
using NguyenNgocThien_Buoi4.Models;

namespace NguyenNgocThien_BanHang.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public StoreController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index(int? categoryId, string searchString, string sortOrder)
        {
            ViewData["PriceSortParam"] = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewData["NameSortParam"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;

            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
                ViewBag.CategoryName = await _context.Categories.Where(c => c.Id == categoryId).Select(c => c.Name).FirstOrDefaultAsync();
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "name":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                default:
                    products = products.OrderBy(p => p.Price);
                    break;
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();

            var cart = _cartService.GetCart();
            ViewBag.CartItemCount = cart.TotalItems;

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy sản phẩm liên quan (cùng danh mục)
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            var cart = _cartService.GetCart();
            ViewBag.CartItemCount = cart.TotalItems;

            return View(product);
        }

        //[HttpPost]
        //public IActionResult AddToCart(int productId, int quantity = 1)
        //{
        //    var product = _context.Products.Find(productId);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    _cartService.AddToCart(product, quantity);
        //    TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng!";

        //    return RedirectToAction("Details", new { id = productId });
        //}
    }
}