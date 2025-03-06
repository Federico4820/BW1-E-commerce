using System.ComponentModel.DataAnnotations;

namespace BW1_E_commerce.Models
{
    public class CartItem
    {
        public Guid IdProd { get; set; } 
        public Guid IdOrder { get; set; }
        public string? ProductName { get; set; } 
        public int Quantity { get; set; }      
        public decimal Price { get; set; }     
        public decimal Total => Quantity * Price;
    }
}
