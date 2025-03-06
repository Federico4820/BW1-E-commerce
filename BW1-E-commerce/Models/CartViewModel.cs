using System.Collections.Generic;

namespace BW1_E_commerce.Models
{
    public class CartViewModel
    {
        public int IdOrder { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Total => Items?.Sum(item => item.Price * item.Quantity) ?? 0;
    }
}