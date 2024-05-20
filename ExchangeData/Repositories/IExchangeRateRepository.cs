using ExchangeData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Repositories
{
    public interface IExchangeRateRepository
    {
        Task<bool> AddRangeAsync(List<ExchangeRate> currencies);
        Task<List<object>> GetByRangeAsync(string isoCode, string dateFrom, string dateTo);
        Task<bool> SaveChangesAsync();
    }
}
