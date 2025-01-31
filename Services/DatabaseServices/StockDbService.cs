using System.Data;
using System.Data.Common;
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

        public async Task<IEnumerable<Stock>> GetStockAsync(int stockId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Stocks"" WHERE Id = @Id";
                return await dbConnection.QueryAsync<Stock>(query, new { Id = stockId });
            };
        }

        public async Task<int> CreateStockAsync(Stock stock)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""Stocks"" (Name) VALUES (@Name)";
                return await dbConnection.ExecuteAsync(query, stock);
            };
        }

        public async Task<int> UpdateStockAsync(Stock updatedStock)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Stocks"" SET Name = @Name WHERE Id = @Id";
                return await dbConnection.ExecuteAsync(query, new { updatedStock.Name, updatedStock.Id });
            }
        }

        public async Task<int> DeleteStock(int stockId)
        {
            using (DbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""Stocks"" WHERE Id = @Id";
                return await dbConnection.ExecuteAsync(query, stockId);
            };
        }
    }
}
