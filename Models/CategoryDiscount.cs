using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class CategoryDiscount
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public required decimal Discount { get; set; }

        [ForeignKey("ProductCategory")]
        public required int ProductCategoryId { get; set; }

        public ProductCategory? ProductCategory { get; set; }

        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime DiscountStartDate { get; set; }

        public DateTime DiscountEndDate { get; set; }

        public required bool IsApplied { get; set; }

        public bool IsManuallyUpdated { get; set; }
    }
}
