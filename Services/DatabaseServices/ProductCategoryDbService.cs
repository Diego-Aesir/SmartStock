using System.Data;
using Dapper;
using SmartStock.Models;

namespace SmartStock.Services.DatabaseServices
{
    public class ProductCategoryDbService
    {
        private readonly string connectionString;

        public ProductCategoryDbService(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateProductCategory(ProductCategory productCategory)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""ProductCategories"" (""Title"", ""Description"") VALUES (@Title, @Description) RETURNING ""Id""";
                return await dbConnection.ExecuteScalarAsync<int>(query, new { productCategory.Title, Description = productCategory.Description ?? "" });
            }
        }

        public async Task<ProductCategory> GetProductCategoryById(int productCategoryId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""ProductCategories"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstAsync<ProductCategory>(query, new { Id = productCategoryId });
            }
        }

        public async Task<IEnumerable<ProductCategory>> GetAllCategories()
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""ProductCategories""";
                return await dbConnection.QueryAsync<ProductCategory>(query);
            }
        }

        public async Task<int> UpdateProductCategory(ProductCategory productCategory)
        {
            var category = await GetProductCategoryById(productCategory.Id);
            if (category == null)
            {
                throw new Exception("This Category does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"UPDATE ""ProductCategories"" SET ""Title"" = @Title, ""Description"" = @Description WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Title = productCategory.Title ?? category.Title, Description = productCategory.Description ?? category.Description, productCategory.Id });
            }
        }
        
        public async Task<int> DeleteProductCategory(int categoryId)
        {
            var category = await GetProductCategoryById(categoryId);
            if (category == null)
            {
                throw new Exception("This Category does not exist");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""ProductCategories"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = category.Id });
            }
        }
    }
}
