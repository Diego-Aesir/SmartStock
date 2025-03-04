using System.Data;
using Dapper;
using SmartStock.Models;
using SmartStock.DTO.CategoryDiscount;

namespace SmartStock.Services.DatabaseServices
{
    public class CategoryDiscountDbService
    {
        private readonly string connectionString;

        public CategoryDiscountDbService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateDiscountCategory(CategoryDiscount categoryDiscount)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"INSERT INTO ""CategoryDiscount"" 
                                (""Title"", ""Discount"", ""ProductCategoryId"", ""CreatedAt"", ""DiscountStartDate"", ""DiscountEndDate"", ""IsApplied"", ""IsManuallyUpdated"")
                                VALUES (@Title, @Discount, @ProductCategoryId, @CreatedAt, @DiscountStartDate, @DiscountEndDate, @IsApplied, @IsManuallyUpdated) RETURNING ""Id""";
                return await dbConnection.ExecuteScalarAsync<int>(query, new
                {
                    Title = categoryDiscount.Title,
                    Discount = categoryDiscount.Discount,
                    ProductCategoryId = categoryDiscount.ProductCategoryId,
                    CreatedAt = categoryDiscount.CreatedAt,
                    DiscountStartDate = categoryDiscount.DiscountStartDate,
                    DiscountEndDate = categoryDiscount.DiscountEndDate,
                    IsApplied = false,
                    IsManuallyUpdated = categoryDiscount.IsManuallyUpdated
                });
            }
        }

        public async Task<int> ManuallyApplyDiscount(int discountId)
        {
            var categoryDiscount = await GetCategoryDiscountById(discountId);
            if (categoryDiscount == null)
            {
                throw new Exception("Category Discount not found");
            }

            if (categoryDiscount.IsApplied == true)
            {
                throw new Exception("Discount is already applied");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string updateProductsQuery = @"
                    UPDATE ""Products""
                    SET ""Price"" = ROUND(""Price"" * (1 - @Discount),2)
                    WHERE ""CategoryId"" = @CategoryId";

                    string updateDiscountQuery = @"
                    UPDATE ""CategoryDiscount""
                    SET ""IsApplied"" = true, ""IsManuallyUpdated"" = true
                    WHERE ""Id"" = @Id";

                    await dbConnection.ExecuteAsync(updateProductsQuery, new
                    {
                        Discount = categoryDiscount.Discount,
                        CategoryId = categoryDiscount.ProductCategoryId
                    }, transaction);

                    await dbConnection.ExecuteAsync(updateDiscountQuery, new
                    {
                        Id = discountId
                    }, transaction);

                    transaction.Commit();
                    dbConnection.Close();
                    return 1;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> ManuallyDisableDiscount(int discountId)
        {
            var categoryDiscount = await GetCategoryDiscountById(discountId);
            if (categoryDiscount == null)
            {
                throw new Exception("Category Discount not found");
            }

            if (categoryDiscount.IsApplied == false)
            {
                throw new Exception("Discount is already disabled");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                dbConnection.Open();
                IDbTransaction transaction = dbConnection.BeginTransaction();
                try
                {
                    string updateProductsQuery = @"
                    UPDATE ""Products""
                    SET ""Price"" = ROUND(""Price"" / (1 - @Discount),2)
                    WHERE ""CategoryId"" = @CategoryId";

                    string updateDiscountQuery = @"
                    UPDATE ""CategoryDiscount""
                    SET ""IsApplied"" = false, ""IsManuallyUpdated"" = true
                    WHERE ""Id"" = @Id";

                    await dbConnection.ExecuteAsync(updateProductsQuery, new
                    {
                        Discount = categoryDiscount.Discount,
                        CategoryId = categoryDiscount.ProductCategoryId
                    }, transaction);

                    await dbConnection.ExecuteAsync(updateDiscountQuery, new
                    {
                        Id = discountId
                    }, transaction);

                    transaction.Commit();
                    dbConnection.Close();
                    return 1;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<CategoryDiscount?> GetCategoryDiscountById(int id)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""CategoryDiscount"" WHERE ""Id""= @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<CategoryDiscount>(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<CategoryDiscount>> GetAllCategoryDiscounts()
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""CategoryDiscount""";
                return await dbConnection.QueryAsync<CategoryDiscount>(query);
            }
        }

        public async Task<CategoryDiscount> UpdateCategoryDiscount(int discountId, UpdateCategoryDiscount categoryDiscount)
        {
            var discountFound = await GetCategoryDiscountById(discountId);
            if (discountFound == null)
            {
                throw new Exception("Category Discount not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                bool valueUpdate = false;
                string query = @"UPDATE ""CategoryDiscount"" SET ""Title"" = @Title, ""Discount"" = @Discount,
                                ""ProductCategoryId"" = @ProductCategoryId, ""DiscountStartDate"" = @DiscountStartDate,
                                ""DiscountEndDate"" =  @DiscountEndDate WHERE ""Id"" = @Id RETURNING *";

                if (categoryDiscount.Discount != discountFound.Discount)
                {
                    await ManuallyDisableDiscount(discountFound.Id);
                    valueUpdate = true;
                }

                CategoryDiscount updatedCategory = await dbConnection.QueryFirstAsync<CategoryDiscount>(query, new
                {
                    Title = categoryDiscount.Title ?? discountFound.Title,
                    Discount = categoryDiscount.Discount ?? discountFound.Discount,
                    ProductCategoryId = categoryDiscount.ProductCategoryId ?? discountFound.ProductCategoryId,
                    DiscountStartDate = categoryDiscount.DiscountStartDate ?? discountFound.DiscountStartDate,
                    DiscountEndDate = categoryDiscount.DiscountEndDate ?? discountFound.DiscountEndDate,
                    Id = discountId
                });

                if (valueUpdate)
                {
                    await ManuallyApplyDiscount(updatedCategory.Id);
                }

                return updatedCategory;
            }
        }

        public async Task<int> DeleteCategoryDiscount(int discountId)
        {
            var categoryDiscount = await GetCategoryDiscountById(discountId);
            if (categoryDiscount == null)
            {
                throw new Exception("Category Discount not found");
            }

            if (categoryDiscount.IsApplied == true)
            {
                await ManuallyDisableDiscount(discountId);
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""CategoryDiscount"" WHERE ""Id""= @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = discountId });
            }
        }
    }
}
