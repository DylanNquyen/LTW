using System.Text.Json.Serialization;

namespace NguyenNgocThien_Buoi4.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [JsonIgnore]
        public decimal TotalAmount => Items.Sum(i => i.Product.Price * i.Quantity);

        [JsonIgnore]
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}