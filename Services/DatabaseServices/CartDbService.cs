using System.Data;
using Dapper;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class CartDbService
    {
        public readonly string connectionString;

        public CartDbService(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Cart> GetCartById(int cartId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Carts"" WHERE ""Id"" = @Id";
                var cart = await dbConnection.QueryFirstAsync<Cart>(query, new {Id = cartId});
                return cart;
            }
        }

        public async Task<int> CreateCart()
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""Carts"" DEFAULT VALUES RETURNING ""Id""";
                return await dbConnection.ExecuteScalarAsync<int>(query);
            }
        }

        public async Task<int> DeleteCart(int cartId)
        {
            var cartExist = await GetCartById(cartId);
            if (cartExist == null)
            {
                throw new Exception("Cart does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""Carts"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new {Id = cartId});
            }
        }
    }
}
