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
        private readonly ProductInCartDbService _productInCartDbService;

        public SalesTransactionDbService(IConfiguration configuration, ProductDbService productDbService,
                                         ProductSoldDbService productSoldDbService, ProductInCartDbService productInCartDbService)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _productDbService = productDbService;
            _productSoldDbService = productSoldDbService;
            _productInCartDbService = productInCartDbService;
        }

        public async Task<int> CreateTransaction(SalesTransaction sale)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""SalesTransactions"" (""ClientId"", ""SoldTime"", ""PaymentMethod"", ""TotalRevenue"", ""SaleDiscount"")
                                    VALUES (@ClientId, @SoldTime, @PaymentMethod, @TotalRevenue, @SaleDiscount)
                                    RETURNING ""Id""";

                return await dbConnection.QueryFirstOrDefaultAsync<int>(query, new
                {
                    ClientId = sale.ClientId,
                    SoldTime = sale.SoldTime,
                    PaymentMethod = sale.PaymentMethod,
                    TotalRevenue = 0,
                    SaleDiscount = sale.SaleDiscount
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

        public async Task<IEnumerable<SalesTransaction>> GetTransactionFromUser(string userId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""SalesTransactions"" WHERE ""ClientId"" = @ClientId";
                return await dbConnection.QueryAsync<SalesTransaction>(query, new { ClientId = userId });
            }
        }

        public async Task<SalesTransaction> TransactionProduct(int salesTransactionId, ProductInCart productInCart)
        {
            var transaction = await GetTransaction(salesTransactionId);
            if (transaction == null)
            {
                throw new Exception("Transaction not found");
            }

            var productExist = await _productDbService.GetProduct(productInCart.ProductId);
            if (productExist == null)
            {
                throw new Exception("Product not found");
            }

            decimal finalPrice = productExist.Price * (1 - productExist.Discount);

            ProductSold productSold = new()
            {
                ProductId = productInCart.ProductId,
                ProductTitle = productExist.Title,
                SalesTransactionId = salesTransactionId,
                Quantity = productInCart.Quantity,
                SoldPrice = finalPrice,
                Discount = productExist.Discount
            };

            await _productSoldDbService.CreateProductSold(productSold);

            await _productInCartDbService.DeleteProductInCart(productInCart.Id);

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""SalesTransactions"" SET ""TotalRevenue"" = @TotalRevenue WHERE ""Id"" = @Id  RETURNING *";
                return await dbConnection.QueryFirstAsync<SalesTransaction>(query, new
                {
                    TotalRevenue = transaction.TotalRevenue + (productSold.SoldPrice * productSold.Quantity),
                    Id = salesTransactionId
                });
            }
        }

        public async Task<SalesTransaction> CompleteTransaction(int salesTransactionId, int cartId) 
        {
            var transaction = await GetTransaction(salesTransactionId);
            if (transaction == null)
            {
                throw new Exception("Transaction not found");
            }

            var products = await _productInCartDbService.GetAllProductsFromCartId(cartId);
            foreach (var product in products)
            {
               await TransactionProduct(salesTransactionId, product);
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                transaction = await GetTransaction(salesTransactionId);

                string query = @"UPDATE ""SalesTransactions"" SET ""TotalRevenue"" = @TotalRevenue WHERE ""Id"" = @Id  RETURNING *";
                return await dbConnection.QueryFirstAsync<SalesTransaction>(query, new
                {
                    TotalRevenue = transaction.TotalRevenue * (1 - transaction.SaleDiscount),
                    Id = salesTransactionId
                });
            }
        }

        public async Task<IEnumerable<SalesTransaction>> GetAllTransactionsWithinDateRange(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddDays(1).AddTicks(-1);

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""SalesTransactions"" WHERE ""SoldTime"" >= @StartDate AND ""SoldTime"" <= @EndDate";
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
