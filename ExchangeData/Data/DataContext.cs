
using ExchangeData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeData.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) :base(options) { }
        DbSet<ExchaneRates> ExchaneRates { get; set; }
    }
}
