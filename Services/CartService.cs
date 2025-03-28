using Microsoft.AspNetCore.Http;
using NguyenNgocThien_BanHang.Data;
using NguyenNgocThien_Buoi4.Models;
using System.Text.Json;

namespace NguyenNgocThien_BanHang.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public Cart GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }

            var cart = JsonSerializer.Deserialize<Cart>(cartJson);

            // Cập nhật thông tin sản phẩm từ database
            foreach (var item in cart.Items)
            {
                var product = _context.Products.Find(item.Product.Id);
                if (product != null)
                {
                    item.Product = product;
                }
            }

            return cart;
        }

        private void SaveCart(Cart cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        public void AddToCart(Product product, int quantity)
        {
            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.Product.Id == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    Product = product,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Product.Id == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Product.Id == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext.Session.Remove(CartSessionKey);
        }
    }
}