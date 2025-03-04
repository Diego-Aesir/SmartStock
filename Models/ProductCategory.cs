using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public ICollection<Products>? Products { get; set; }

        public CategoryDiscount? CategoryDiscount { get; set; }
    }
}
