
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
            if (currencies == null || !currencies.Any())
            {
                throw new ArgumentException("The currencies list cannot be null or empty.", nameof(currencies));
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

        public async Task<List<ExchangeRateDto>> GetByRangeAsync(string isoCode, string from, string to)
        {
            string requestedCache = $"Requested_key_{isoCode}_{from}_{to}";

            //Delete cache if there is no request for more than 2 minute
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            ArgumentException.ThrowIfNullOrEmpty(isoCode);
            if (!DateTime.TryParse(from, out DateTime dateFrom) ||
                !DateTime.TryParse(to, out DateTime dateTo))
            {
                throw new ArgumentException("Invalid date format. Please use 'yyyy-MM-dd'.");
            }

            if (_memoryCache.TryGetValue(requestedCache, out List<ExchangeRateDto> cachedData))
            {
                return cachedData;
            }

            var existingRates = await _context.Rates
                .Include(er => er.ISO)
                .Where(er => er.ISO.Code == isoCode.ToLower() && er.RateDate >= dateFrom && er.RateDate <= dateTo)
                .ToListAsync();

            if (!existingRates.Any())
            {
                var fetchedRates = await _client.PostAsync(isoCode, from, to);

                _memoryCache.Set(requestedCache, fetchedRates, cacheEntryOptions);

                List<ExchangeRate> exchangeRates = MapCollection<ExchangeRate, ExchangeRateDto>(fetchedRates);

                try
                {
                    await AddRangeAsync(exchangeRates);
                    return fetchedRates;

                }
                catch (Exception ex) 
                {
                    throw new ExchangeRateException(ex.Message);

                }

            }
            if (CheckRange(dateFrom, dateTo) > existingRates.Count) 
            {
                var fetchedRates = await _client.PostAsync(isoCode, from, to);

                _memoryCache.Set(requestedCache, fetchedRates, cacheEntryOptions);

                List<ExchangeRate> newExchangeRates = MapCollection<ExchangeRate, ExchangeRateDto>(fetchedRates);

                 newExchangeRates.RemoveAll(item => existingRates.Contains(item));

                try
                {
                    await AddRangeAsync(newExchangeRates);
                    return fetchedRates;

                }
                catch (Exception ex)
                {
                    throw new ExchangeRateException(ex.Message);

                }
            }
            return MapCollection<ExchangeRateDto, ExchangeRate>(existingRates);
        }

        private int CheckRange(DateTime dateFrom, DateTime dateTo)
        {
            var allDates = Enumerable.Range(0, (dateTo - dateFrom).Days + 1)
                .Select(offset => dateFrom.AddDays(offset))
                .ToList();

            return allDates.Count;
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
