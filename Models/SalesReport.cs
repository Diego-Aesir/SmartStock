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

        public decimal TotalRevenue => SalesData.Sum(s => s.TotalRevenue);

        public int TotalQuantitySold => SalesData.Sum(s => s.QuantitySold);

        public decimal AveragePriceSold => SalesData.Average(s => s.SoldPrice);

        public decimal PercentageSoldWithDiscount => SalesData.Count() == 0 ? 0 : SalesData.Count(s => s.PriceDifference > 0) / (decimal)SalesData.Count() * 100;
    }
}
