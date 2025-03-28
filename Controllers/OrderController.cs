using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenNgocThien_BanHang.Data;
using System.Security.Claims;

namespace NguyenNgocThien_BanHang.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Pending" || order.Status == "Processing")
            {
                order.Status = "Cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đơn hàng đã được hủy thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng ở trạng thái hiện tại!";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}