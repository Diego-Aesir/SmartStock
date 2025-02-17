using System.Data;
using Dapper;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class ProductInCartDbService
    {
        private readonly string connectionString;
        private readonly CartDbService _cartDbService;
        private readonly ProductDbService _productDbService;

        public ProductInCartDbService(IConfiguration configuration, CartDbService cartDbService, ProductDbService productDbService)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _cartDbService = cartDbService;
            _productDbService = productDbService;
        }

        public async Task<int> CreateProductInCart(ProductInCart productInCart)
        {
            var cartExists = await _cartDbService.GetCartById(productInCart.CartId);
            if (cartExists == null)
            {
                throw new Exception("Cart does not exist");
            }

            var productExists = await _productDbService.GetProduct(productInCart.ProductId);
            if (productExists == null)
            {
                throw new Exception("Product does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""ProductsInCart"" (""CartId"", ""ProductId"", ""Quantity"") VALUES (@CartId, @ProductId, @Quantity) RETURNING ""Id""";
                return await dbConnection.ExecuteScalarAsync<int>(query, new {CartId = productInCart.CartId, ProductId = productInCart.ProductId, Quantity = productInCart.Quantity});
            }
        }

        public async Task<ProductInCart> GetProductInCartById(int productId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""ProductsInCart"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstAsync<ProductInCart>(query, new { Id = productId });
            }
        }

        public async Task<IEnumerable<ProductInCart>> GetAllProductsFromCartId(int cartId)
        {
            var cartExists = await _cartDbService.GetCartById(cartId);
            if (cartExists == null)
            {
                throw new Exception("Cart does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""ProductsInCart"" WHERE ""CartId"" = @CartId";
                return await dbConnection.QueryAsync<ProductInCart>(query, new { CartId = cartId });
            }
        }

        public async Task<ProductInCart?> UpdateProductQuantity(int productId, int newQuantity)
        {
            var productExists = await GetProductInCartById(productId);
            if (productExists == null)
            {
                throw new Exception("Product in Cart does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""ProductsInCart"" SET ""Quantity"" = @Quantity WHERE ""Id"" = @Id RETURNING *";
                return await dbConnection.QueryFirstOrDefaultAsync<ProductInCart>(query, new { Quantity = newQuantity, Id = productId });
            }
        }

        public async Task<int> DeleteProductInCart(int productId)
        {
            var productExists = await GetProductInCartById(productId);
            if (productExists == null)
            {
                throw new Exception("Product in Cart does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""ProductsInCart"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = productId });
            }
        }
    }
}
