using System.Data;
using Dapper;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class SalesTransactionDbService
    {
        private readonly string connectionString;
        private readonly ProductDbService _productDbService;
        private readonly ProductSoldDbService _productSoldDbService;

        public SalesTransactionDbService(IConfiguration configuration, ProductDbService productDbService, ProductSoldDbService productSoldDbService)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _productDbService = productDbService;
            _productSoldDbService = productSoldDbService;
        }

        public async Task<int> CreateTransaction(SalesTransaction sale)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""SalesTransactions"" (""ClientId"", ""SoldTime"", ""PaymentMethod"", ""TotalRevenue"", ""PriceDifference"")
                                    VALUES (@ClientId, @SoldTime, @PaymentMethod, @TotalRevenue, @PriceDifference)
                                    RETURNING ""Id""";

                return await dbConnection.QueryFirstOrDefaultAsync<int>(query, new
                {
                    ClientId = sale.ClientId,
                    SoldTime = sale.SoldTime,
                    PaymentMethod = sale.PaymentMethod,
                    TotalRevenue = 0,
                    PriceDifference = 0
                });
            }
        }

        public async Task<SalesTransaction?> GetTransaction(int saleId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""SalesTransactions"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<SalesTransaction>(query, new { Id = saleId });
            }
        }

        public async Task<SalesTransaction> AddProductSoldToTransaction(ProductSold product)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                var transaction = await GetTransaction(product.SalesTransactionId);
                if (transaction == null)
                {
                    throw new Exception("Transaction not found");
                }

                var productExist = await _productDbService.GetProduct(product.ProductId);
                if (productExist == null)
                {
                    throw new Exception("Product not found");
                }

                string query = @"UPDATE ""SalesTransactions"" SET ""TotalRevenue"" = @TotalRevenue, ""PriceDifference"" = @PriceDifference WHERE ""Id"" = @Id  RETURNING *";
                await _productSoldDbService.CreateProductSold(product);

                return await dbConnection.QueryFirstAsync<SalesTransaction>(query, new
                {
                    TotalRevenue = transaction.TotalRevenue + (product.SoldPrice * product.Quantity),
                    PriceDifference = productExist.Price - product.SoldPrice,
                    Id = product.SalesTransactionId
                });
            }
        }
        

        public async Task<IEnumerable<SalesTransaction>> GetAllTransactionsWithinDateRange(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddDays(1).AddTicks(-1);

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""SalesTransaction"" WHERE ""SoldTime"" >= @StartDate AND ""SoldTime"" <= @EndDate";
                return await dbConnection.QueryAsync<SalesTransaction>(query, new { StartDate = startDate, EndDate = endDate });
            }
        }

        public async Task<int> DeleteTransaction(int saleId)
        {
            var transaction = await GetTransaction(saleId);
            if (transaction == null)
            {
                throw new Exception("Transaction not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""SalesTransactions"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = saleId });
            }
        }
    }
}
