
using ExchangeData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExchangeData.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) :base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExchangeRate>()
                .Property(e => e.Rate)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<ExchangeRate>()
                .HasOne(e => e.ISO)
                .WithMany(e => e.ExchangeRates)
                .HasForeignKey(e => e.CodeID);

            modelBuilder.Entity<ExchangeRate>()
                .Property(e => e.Diff)
                .HasColumnType("decimal(18, 2)");
        }
        public DbSet<ExchangeRate> Rates { get; set; }
        public DbSet<IsoCode> IsoCodes { get; set; }
        public DbSet<Archive> Archives { get; set; }
    }
}
