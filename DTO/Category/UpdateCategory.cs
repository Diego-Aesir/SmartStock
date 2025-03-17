namespace SmartStock.DTO.Category
{
    public class UpdateCategory
    {
        public required int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}
