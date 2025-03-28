namespace NguyenNgocThien_Buoi4.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal Total => UnitPrice * Quantity;
        public Product Product { get; set; }
    }
}