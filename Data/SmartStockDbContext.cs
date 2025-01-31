using Microsoft.EntityFrameworkCore;
using SmartStock.Models;

namespace SmartStock.Data
{
    public class SmartStockDbContext : DbContext
    {
        public SmartStockDbContext(DbContextOptions<SmartStockDbContext> options) : base(options) { }

        DbSet<Stock> Stocks { get; set; }

        DbSet<Products> Products { get; set; }

        DbSet<ProductCategory> ProductCategories { get; set; }

        DbSet<SalesReport> SalesReports { get; set; }

        DbSet<SalesTransaction> SalesTransactions { get; set; }
    }
}
