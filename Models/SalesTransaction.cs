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

        [ForeignKey("ProductSold")]
        public ICollection<ProductSold> ProductSold { get; set; }

        public DateTime SoldTime { get; set; } = DateTime.UtcNow;

        public required string PaymentMethod { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal PriceDifference { get; set; }
    }
}
