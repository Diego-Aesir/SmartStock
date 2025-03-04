using System.Data;
using Dapper;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class ProductSoldDbService
    {
        private readonly string connectionString;
        private readonly ProductDbService _productDbService;

        public ProductSoldDbService(IConfiguration configuration, ProductDbService productDbService)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _productDbService = productDbService;
        }

        public async Task<int> CreateProductSold(ProductSold productSold)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""ProductsSold"" (""SalesTransactionId"", ""Quantity"", ""SoldPrice"", ""ProductId"", ""Discount"")
                                    VALUES (@SalesTransactionId, @Quantity, @SoldPrice, @ProductId, @Discount)
                                    RETURNING ""Id""";

                
                var product = await _productDbService.GetProduct(productSold.ProductId);
                if (product == null)
                {
                    throw new Exception("Product was not found on the database");
                }

                return await dbConnection.QueryFirstOrDefaultAsync<int>(query, new
                {
                    SalesTransactionId = productSold.SalesTransactionId,
                    Quantity = productSold.Quantity,
                    SoldPrice = productSold.SoldPrice,
                    ProductId = productSold.ProductId,
                    Discount = productSold.Discount,
                });
            }
        }

        public async Task<ProductSold?> GetProductSold(int productSoldId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""ProductsSold"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<ProductSold>(query, new { Id = productSoldId });
            }
        }

        public async Task<IEnumerable<ProductSold>> GetAllProductsSoldByTransactionId(int transactionId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""ProductsSold"" WHERE ""SalesTransactionId"" = @TransactionId";
                return await dbConnection.QueryAsync<ProductSold>(query, new {  TransactionId = transactionId });
            }
        }

        public async Task<int> DeleteProductSold(int saleId)
        {
            var productSold = await GetProductSold(saleId);
            if (productSold == null)
            {
                throw new Exception("Product Sold not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""ProductsSold"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = saleId });
            }
        }
    }
}
