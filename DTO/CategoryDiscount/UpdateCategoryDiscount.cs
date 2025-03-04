namespace SmartStock.DTO.CategoryDiscount
{
    public class UpdateCategoryDiscount
    {
        public required int Id { get; set; }

        public string? Title { get; set; }

        public decimal? Discount { get; set; }

        public int? ProductCategoryId { get; set; }

        public DateTime? DiscountStartDate { get; set; }

        public DateTime? DiscountEndDate { get; set; }
    }
}
