using System.Data;
using Dapper;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class StockDbService
    {
        private readonly string connectionString;
        public StockDbService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Stock> GetStockAsync(int stockId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Stocks"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstAsync<Stock>(query, new { Id = stockId });
            };
        }

        public async Task<IEnumerable<Stock>> GetAllStockAsync()
        {
            using(IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Stocks""";
                return await dbConnection.QueryAsync<Stock>(query);
            }
        }

        public async Task<int> CreateStockAsync(Stock stock)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""Stocks"" (""Name"") VALUES (@Name) RETURNING ""Id""";
                return await dbConnection.ExecuteScalarAsync<int>(query, stock);
            };
        }

        public async Task<Stock?> UpdateStockNameAsync(int id, string name)
        {
            var stockExists = await GetStockAsync(id);
            if (stockExists == null)
            {
                throw new Exception("Stock does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Stocks"" SET ""Name"" = @Name WHERE ""Id"" = @Id RETURNING *";
                return await dbConnection.QueryFirstOrDefaultAsync<Stock>(query, new { name, id });
            }
        }

        public async Task<int> DeleteStock(int stockId)
        {
            var stockExists = await GetStockAsync(stockId);
            if (stockExists == null)
            {
                throw new Exception("Stock does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""Stocks"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = stockId });
            };
        }
    }
}
