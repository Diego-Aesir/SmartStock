using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public required ICollection<ProductInCart> Products { get; set; } = new List<ProductInCart>();
    }
}
