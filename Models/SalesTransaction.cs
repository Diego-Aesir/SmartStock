using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class SalesTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ClientId")]
        public required string ClientId { get; set; }

        [ForeignKey("Products")]
        public required int ProductId { get; set; }

        public Products Products { get; set; }

        public DateTime SoldTime { get; set; } = DateTime.UtcNow;

        public int QuantitySold { get; set; }

        public decimal SoldPrice { get; set; }

        public decimal TotalRevenue => SoldPrice * QuantitySold;

        public decimal PriceDifference => Products.Price - SoldPrice;
    }
}
