using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }

        public required ICollection<Products> Products { get; set; } = new List<Products>();
    }
}
