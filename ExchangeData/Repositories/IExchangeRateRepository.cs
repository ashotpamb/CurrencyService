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
        Task<ExchaneRates> AddAsync(ExchaneRates currency);
        Task<ExchaneRates> GetAsync(long id);
        Task<ExchaneRates> GetByRangeAsync(string isoCode, string dateFrom, string dateTo);
    }
}
