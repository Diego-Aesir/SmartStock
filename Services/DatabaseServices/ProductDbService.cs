using System.Data;
using Dapper;
using Microsoft.CodeAnalysis;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class ProductDbService
    {
        private readonly string connectionString;

        public ProductDbService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateProduct(Products product)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""Products"" (""Title"", ""Description"", ""CategoryId"", ""StockId"", ""Price"", ""QuantityInStock"", ""AddedTime"", ""Discount"") 
                         VALUES (@Title, @Description, @CategoryId, @StockId, @Price, @QuantityInStock, @AddedTime, @Discount) 
                         RETURNING ""Id""";
                return await dbConnection.ExecuteScalarAsync<int>(query, product);
            }
        }

        public async Task<Products?> GetProduct(int productId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Products"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { Id = productId });
            }
        }

        public async Task<IEnumerable<Products>> GetAllProductsByRecentAddition()
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Products"" ORDER BY ""AddedTime"" DESC";
                return await dbConnection.QueryAsync<Products>(query);
            }
        }

        public async Task<IEnumerable<Products>> GetAllProductsFromCategoryId(int categoryId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Products"" WHERE ""CategoryId"" = @CategoryId";
                return await dbConnection.QueryAsync<Products>(query, new { CategoryId = categoryId });
            }
        }

        public async Task<IEnumerable<Products>> FindProductByTitle(string title)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""Products"" WHERE ""Title"" = @Title";
                return await dbConnection.QueryAsync<Products>(query, new { Title = title });
            }
        }

        public async Task<int> DeleteProduct(int productId)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""Products"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = productId });
            }
        }

        public async Task<Products?> ChangeProductQuantity(int productId, int newQuantity)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""QuantityInStock"" = @QuantityInStock WHERE ""Id"" = @Id RETURNING *";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { QuantityInStock = newQuantity, Id = productId });
            }
        }

        public async Task<Products?> ChangeProductTitle(int productId, string newTitle)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""Title"" = @Title WHERE ""Id"" = @Id RETURNING *";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { Title = newTitle ?? product.Title, Id = productId });
            }
        }

        public async Task<Products?> ChangeProductDescription(int productId, string? newDescription)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""Description"" = @Description WHERE ""Id"" = @Id RETURNING *";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new
                {
                    Description = newDescription ?? product.Description,
                    Id = productId
                });
            }
        }

        public async Task<Products?> ChangeProductCategory(int productId, int newCategoryId)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""CategoryId"" = @CategoryId WHERE ""Id"" = @Id RETURNING *";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { CategoryId = newCategoryId, Id = productId });
            }
        }

        public async Task<Products?> ChangeProductStock(int productId, int newStockId)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""StockId"" = @StockId WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { StockId = newStockId, Id = productId });
            }
        }

        public async Task<Products?> ChangeProductPrice(int productId, decimal newPrice)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""Price"" = @Price WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { Price = newPrice, Id = productId });
            }
        }

        public async Task<Products?> ChangeProductDiscount(int productId, decimal discount)
        {
            var product = await GetProduct(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""Products"" SET ""Discount"" = @Discount WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<Products>(query, new { Discount = discount, Id = productId });
            }
        }

        public async Task<Products?> UpdateProduct(Products updatedProduct)
        {

            var product = await GetProduct(updatedProduct.Id) ?? throw new Exception("Product not found");

            if (product.Title != updatedProduct.Title)
            {
                await ChangeProductTitle(updatedProduct.Id, updatedProduct.Title);
            }

            if (product.Description != updatedProduct.Description)
            {
                await ChangeProductDescription(updatedProduct.Id, updatedProduct.Description);
            }

            if (product.CategoryId != updatedProduct.CategoryId)
            {
                await ChangeProductCategory(updatedProduct.Id, updatedProduct.CategoryId);
            }

            if (product.StockId != updatedProduct.StockId)
            {
                await ChangeProductStock(updatedProduct.Id, updatedProduct.StockId);
            }

            if (product.Price != updatedProduct.Price)
            {
                await ChangeProductPrice(updatedProduct.Id, updatedProduct.Price);
            }

            if (product.QuantityInStock != updatedProduct.QuantityInStock)
            {
                await ChangeProductQuantity(updatedProduct.Id, updatedProduct.QuantityInStock);
            }

            if (product.Discount != updatedProduct.Discount) 
            {
                await ChangeProductDiscount(updatedProduct.Id, updatedProduct.Discount);
            }

            return await GetProduct(updatedProduct.Id);
        }
    }
}
