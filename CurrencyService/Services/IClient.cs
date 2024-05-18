using CurrencyService.Domain;

namespace CurrencyService.Services
{
    public interface IClient
    {
        Task<List<ExchangeRate>> PostAsync(string iso, string from, string to);
    }
}
