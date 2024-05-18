
using ExchangeData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Repositories
{
    public class ExchangeRateRepository : IExchangeRateRepository
    {
        public async Task<ExchaneRates> AddAsync(ExchaneRates currency)
        {
            await Console.Out.WriteLineAsync("HEllo");
            return default;
        }

        public async Task<ExchaneRates> GetAsync(long id)
        {
            await Console.Out.WriteLineAsync("HEllo");
            return default;
        }

        public async Task<ExchaneRates> GetByRangeAsync(string isoCode, string dateFrom, string dateTo)
        {
            throw new NotImplementedException();
        }
    }
}
