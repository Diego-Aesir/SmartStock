using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        [ForeignKey("ProductCategory")]
        public required int CategoryId { get; set; }

        public ProductCategory? Category { get; set; }

        [ForeignKey("Stock")]
        public required int StockId { get; set; }

        public Stock? Stock { get; set; }

        public required decimal Price { get; set; }

        public required int QuantityInStock { get; set; }

        [Required(ErrorMessage = "The Added Time field is required.")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid DateTime format.")]
        public required DateTime AddedTime { get; set; }
    }
}
