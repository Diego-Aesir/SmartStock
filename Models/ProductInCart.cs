using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class ProductInCart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Cart Cart { get; set; }

        [ForeignKey("Cart")]
        public required int CartId { get; set; }

        [ForeignKey("Products")]
        public required int ProductId { get; set; }

        public Products Products { get; set; }

        public required int Quantity { get; set; }
    }
}
