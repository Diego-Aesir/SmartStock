using SmartStock.Models;

namespace SmartStock.DTO.Product
{
    public class ProductIForm
    {
        public int? Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public required int CategoryId { get; set; }

        public required int StockId { get; set; }

        public required decimal Price { get; set; }

        public decimal? Discount { get; set; }

        public required int QuantityInStock { get; set; }

        public required DateTime AddedTime { get; set; }

        public IFormFile? Photo { get; set; }
    
        public async Task<Products> TurnIFormIntoClass()
        {
            string photoUrl = "";
            if(Photo != null)
            {
                photoUrl = await SavePhoto(Photo);
            }

            Products newProduct = new()
            {
                Title = Title,
                Description = Description ?? string.Empty,
                CategoryId = CategoryId,
                StockId = StockId,
                Price = Price,
                Discount = Discount ?? 0,
                QuantityInStock = QuantityInStock,
                AddedTime = AddedTime,
                Photo = photoUrl
            };

            return newProduct;
        }

        private async Task<string> SavePhoto(IFormFile photo)
        {
            var fileName = Guid.NewGuid().ToString() + photo.FileName;
            
            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }
            
            var filePath = Path.Combine(uploadDirectory, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            return fileName;
        }

        public void RemovePhoto(string fileName)
        {
            fileName = fileName.TrimStart('/');
            Console.WriteLine(fileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
            Console.WriteLine(filePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                throw new FileNotFoundException("File not exists");
            }
        }
    }
}
