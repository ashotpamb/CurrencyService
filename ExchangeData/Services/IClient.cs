using ExchangeData.Dtos;
using ExchangeDate.Domain;

namespace ExchangeData.Services
{
    public interface IClient
    {
        Task<List<ExchangeRateDto>> PostAsync(string iso, string from, string to);
    }
}
