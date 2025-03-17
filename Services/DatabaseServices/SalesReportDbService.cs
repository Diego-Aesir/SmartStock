using Dapper;
using SmartStock.Models;
using System.Data;

namespace SmartStock.Services.DatabaseServices
{
    public class SalesReportDbService
    {
        private readonly string connectionString;
        private readonly SalesTransactionDbService _salesTransaction;
        private readonly ProductSoldDbService _productSold;

        private readonly ILogger<SalesReportDbService> _logger;

        public SalesReportDbService(IConfiguration configuration, SalesTransactionDbService salesTransaction, ProductSoldDbService productSold, ILogger<SalesReportDbService> logger)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _salesTransaction = salesTransaction;
            _productSold = productSold;
            _logger = logger;
        }

        public async Task<int> CreateReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
                {


                    string query = @"INSERT INTO ""SalesReports"" (""StartDate"", ""EndDate"", ""TotalRevenue"", ""TotalQuantitySold"", ""AveragePriceSold"", ""PercentageSoldWithDiscount"", ""MostSoldId"", ""BestClientId"")
                                    VALUES (@StartDate, @EndDate, @TotalRevenue, @TotalQuantitySold, @AveragePriceSold, @PercentageSoldWithDiscount, @MostSoldId, @BestClientId)
                                    RETURNING ""Id""";

                    var transactions = await _salesTransaction.GetAllTransactionsWithinDateRange(startDate, endDate);
                    List<ProductSold> productSolds = new List<ProductSold>();

                    foreach (var transaction in transactions)
                    {
                        var productsSold = await _productSold.GetAllProductsSoldByTransactionId(transaction.Id);
                        foreach (var product in productsSold)
                        {
                            productSolds.Add(product);
                        }
                    }

                    return await dbConnection.QueryFirstOrDefaultAsync<int>(query, new
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalRevenue = transactions.Sum(x => x.TotalRevenue),
                        TotalQuantitySold = productSolds.Sum(x => x.Quantity),
                        AveragePriceSold = productSolds.Average(x => x.SoldPrice),
                        PercentageSoldWithDiscount = productSolds.Count() == 0 ? 0 : productSolds.Count(x => x.Discount > 0) / transactions.Count() * 100,
                        MostSoldId = productSolds.GroupBy(x => x.ProductId)
                                             .OrderByDescending(g => g.Sum(x => x.Quantity))
                                             .FirstOrDefault()?.Key,
                        BestClientId = transactions.GroupBy(x => x.ClientId)
                                               .OrderByDescending(g => g.Sum(x => x.TotalRevenue))
                                               .FirstOrDefault()?.Key
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<SalesReport?> GetReport(int reportId)
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""SalesReports"" WHERE ""Id"" = @Id";
                return await dbConnection.QueryFirstOrDefaultAsync<SalesReport>(query, new { Id = reportId });
            }
        }

        public async Task<IEnumerable<SalesReport?>> GetAllReports()
        {
            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"SELECT * FROM ""SalesReports""";
                return await dbConnection.QueryAsync<SalesReport>(query);
            }
        }

        public async Task<int> DeleteReport(int reportId)
        {
            var report = await GetReport(reportId);
            if (report == null)
            {
                throw new Exception("Report not found");
            }

            using (IDbConnection dbConnection = new Npgsql.NpgsqlConnection(connectionString))
            {
                string query = @"DELETE FROM ""SalesReports"" WHERE ""Id"" = @Id";
                return await dbConnection.ExecuteAsync(query, new { Id = reportId });
            }
        }
    }
}
