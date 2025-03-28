using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenNgocThien_BanHang.Data;
using NguyenNgocThien_BanHang.Services;
using NguyenNgocThien_Buoi4.Models;
using System.Security.Claims;

namespace NguyenNgocThien_BanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            _cartService.AddToCart(product, quantity);
            TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng!";

            // Chuyển hướng về trang chi tiết sản phẩm nếu được gọi từ đó
            if (Request.Headers["Referer"].ToString().Contains("/Store/Details/"))
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            return RedirectToAction("Index", "Store");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                _cartService.RemoveFromCart(productId);
            }
            else
            {
                _cartService.UpdateQuantity(productId, quantity);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            // Nếu người dùng đã đăng nhập, lấy thông tin người dùng
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    ViewBag.UserName = user.FullName;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserPhone = user.PhoneNumber;
                    ViewBag.UserAddress = user.Address;
                }
            }

            return View(cart);
        }

        [HttpPost]
        public IActionResult PlaceOrder(Order order)
        {
            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index");
            }

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                order.UserId = userId;
            }
            else
            {
                // Nếu không đăng nhập, tạo một ID tạm thời
                order.UserId = "guest";
            }

            order.OrderDate = DateTime.Now;
            order.Status = "Pending";
            order.TotalAmount = cart.TotalAmount;

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Thêm chi tiết đơn hàng
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }
            _context.SaveChanges();

            // Xóa giỏ hàng sau khi đặt hàng
            _cartService.ClearCart();

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}