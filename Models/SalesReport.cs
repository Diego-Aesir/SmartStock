using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class SalesReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ICollection<SalesTransaction> SalesData { get; set; } = new List<SalesTransaction>();

        public decimal TotalRevenue { get; set; }

        public int TotalQuantitySold { get; set; }

        public decimal AveragePriceSold { get; set; }

        public decimal PercentageSoldWithDiscount { get; set; }

        public int MostSoldId { get; set; }

        public required string BestClientId { get; set; }
    }
}
