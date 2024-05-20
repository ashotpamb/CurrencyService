
using AutoMapper;
using AutoMapper.Internal;
using ExchangeData.Data;
using ExchangeData.Dtos;
using ExchangeData.Entities;
using ExchangeData.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Repositories
{
    public class ExchangeRateRepository : IExchangeRateRepository
    {
        private readonly IClient _client;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _memoryCache;

        public ExchangeRateRepository(IClient client, DataContext context, IMapper mapper,IMemoryCache memoryCache)
        {
            _client = client;
            _context = context;
            _mapper = mapper;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Bulk add Exchange Rate into database
        /// Add ISO codes into database if codes not exist
        /// </summary>
        /// <param name="currencies"></param>
        /// <returns> Return true if data has been added </returns>
        public async Task<bool> AddRangeAsync(List<ExchangeRate> currencies)
        {
            ArgumentNullException.ThrowIfNull(currencies);
            var distinctIsoCodes = currencies.Select(er => er.ISO.Code).Distinct().Where(code => code != null).ToList();
            var isoCodeDictionary = new Dictionary<string, IsoCode>();

            foreach (var isoCode in distinctIsoCodes)
            {
                var existingIsoCode = _context.IsoCodes.FirstOrDefault(ic => ic.Code == isoCode);
                if (existingIsoCode == null)
                {
                    existingIsoCode = new IsoCode { Code = isoCode };
                    _context.IsoCodes.Add(existingIsoCode);
                    await _context.SaveChangesAsync();
                }
                isoCodeDictionary[isoCode] = existingIsoCode;
            }

            foreach (var currency in currencies)
            {
                if (currency.ISO != null && currency.ISO.Code != null && isoCodeDictionary.TryGetValue(currency.ISO.Code, out var isoCode))
                {
                    currency.ISO = isoCode;
                    currency.CodeID = isoCode.Id;
                }
                else
                {
                    currency.ISO = null;
                    currency.CodeID = null;
                }
            }
            await _context.Rates.AddRangeAsync(currencies);
            await SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves exchange rate data from the database or an external source based on the specified ISO code and date range.
        /// </summary>
        /// <param name="isoCode">The ISO currency code.</param>
        /// <param name="from">The start date of the range in the format 'yyyy-MM-dd'.</param>
        /// <param name="to">The end date of the range in the format 'yyyy-MM-dd'.</param>
        /// <returns>A list of serialized exchange rate data objects. If no data is found, an empty list is returned.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the 'isoCode' parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the 'from' or 'to' date parameters are in an invalid format.</exception>
        /// <exception cref="ExchangeRateException">Thrown when an error occurs while fetching or processing exchange rate data.</exception>

        public async Task<List<object>> GetByRangeAsync(string isoCode, string from, string to)
        {
            string requestedCache = $"Requested_key_{isoCode}_{from}_{to}";

            //Delete cache if there is no request for more than 2 minute
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            ArgumentNullException.ThrowIfNull(isoCode);

            if (!DateTime.TryParseExact(from, "yyyy-mm-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dateFrom) ||
                !DateTime.TryParseExact(to, "yyyy-mm-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dateTo))
            {
                throw new ArgumentException("Invalid date format. Please use 'yyyy-MM-dd'.");
            }
            await Console.Out.WriteLineAsync(requestedCache);

            if (_memoryCache.TryGetValue(requestedCache, out List<ExchangeRateDto> cachedData))
            {
                return cachedData.Select(e => e.Serialize()).ToList();
            }

            var existingRates = await _context.Rates
                .Include(er => er.ISO)
                .Where(er => er.ISO.Code == isoCode && (er.CodeID == null || (er.RateDate >= dateFrom && er.RateDate <= dateTo)))
                .ToListAsync();

            if (!existingRates.Any())
            {
                var fetchedRates = await _client.PostAsync(isoCode, from, to);

                _memoryCache.Set(requestedCache, fetchedRates, cacheEntryOptions);

                List<ExchangeRate> exchangeRates = MapCollection<ExchangeRate, ExchangeRateDto>(fetchedRates);

                try
                {
                    await AddRangeAsync(exchangeRates);
                    return fetchedRates.Select(s => s.Serialize()).ToList();

                }
                catch (Exception ex) 
                {
                    throw new ExchangeRateException(ex.Message);

                }

            }
            var check = CheckRange(existingRates, dateFrom, dateTo);
            return default;
    

        }

        private List<DateTime> CheckRange(List<ExchangeRate> exchangeRateDtos, DateTime dateFrom, DateTime dateTo)
        {
            var maxDate = exchangeRateDtos.Max(e => e.RateDate);
            var minDate = exchangeRateDtos.Min(e => e.RateDate);

            var allDates = Enumerable.Range(0, (dateTo - dateFrom).Days + 1)
                .Select(offset => dateFrom.AddDays(offset))
                .ToList();
            return default;
        }

        /// <summary>
        /// Map collections (Left source TSource) (rigth destination TDestination)
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="dataToMap"></param>
        /// <returns>Return new Destination object</returns>
        private List<TDestination> MapCollection<TDestination, TSource>(List<TSource> dataToMap)
        {
            List<TDestination> mappedData = new List<TDestination>();

            foreach (var rate in dataToMap)
            {
                var data = _mapper.Map<TDestination>(rate);
                mappedData.Add(data);

            }
            return mappedData;
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;

            }
            catch (Exception ex) 
            { 
                throw new ExchangeRateException(ex.Message);
            }
        }
    }
}
