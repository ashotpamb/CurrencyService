
using AutoMapper;
using Castle.Core.Logging;
using ExchangeData.Data;
using ExchangeData.Dtos;
using ExchangeData.Entities;
using ExchangeData.Profile;
using ExchangeData.Repositories;
using ExchangeData.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel.DataAnnotations.Schema;

namespace CurrencyServiceTests
{
    [TestClass]
    public class ExchangeRepositoryTests
    {
        private ServiceProvider _serviceProvider;
        private IExchangeRateRepository _exchangeRateRepository;
        private DataContext _dataContext;
        private Mock<IClient> _client;

        [TestInitialize]
        public void Setup()
        {
            Console.WriteLine("Test Stated");
            var services = new ServiceCollection();
            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
            services.AddMemoryCache();
            services.AddLogging();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<ExchangeRateMapper>();
            });
            _client = new Mock<IClient>();
            services.AddScoped<IClient>(_ => _client.Object);
            services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
            services.AddScoped<IArchive, ArchiveService>();
            
            _serviceProvider = services.BuildServiceProvider();
            _exchangeRateRepository = _serviceProvider.GetRequiredService<IExchangeRateRepository>();
            _dataContext = _serviceProvider.GetRequiredService<DataContext>();


        }

        [TestMethod]
        public async Task AddRangeAsync_ShouldReturnTrueAndAddDataToDb()
        {

            // Arrange
            var isoCode = new IsoCode { Code = "USD" };

            var currencies = new List<ExchangeRate>
            {
                new ExchangeRate { ID = 1, Rate = 1.2m, Amount = 1, ISO = isoCode, RateDate = new DateTime(2024, 5, 20) },
                new ExchangeRate { ID = 2, Rate = 1.5m, Amount = 1, ISO = isoCode, RateDate = new DateTime(2024, 5, 21) }
            };

            // Act
            var result = await _exchangeRateRepository.AddRangeAsync(currencies);

            // Assert
            Assert.IsTrue(result);
            var addedCurrencies = await _dataContext.Rates.ToListAsync();
            Assert.AreEqual(2, addedCurrencies.Count);
            Assert.AreEqual(1.2m, addedCurrencies[0].Rate);
            Assert.AreEqual(1.5m, addedCurrencies[1].Rate);
        }

        [TestMethod]
        public async Task AddRangeAsync_ThrowArgumentExceptionIfDataIsNull()
        {
            //Arrange
            var currencies = new List<ExchangeRate>();
                 
            // Assert Act
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _exchangeRateRepository.AddRangeAsync(currencies);
            });
        }

        [TestMethod]
        public async Task GetDataByRangeAsync_ThrowIfDateFormatIsIncorrect()
        {
            //Arrange
            string isoCode = "USD";
            string dateFrom = "2024-01-01";
            string dateTo = "20240026";

            //Assert Act 

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _exchangeRateRepository.GetByRangeAsync(isoCode, dateFrom, dateTo);
            });
        }

        [TestMethod]
        public async Task GetDataByRangeAsync_ShouldReturnData()
        {
            //Arrange
            string isoCode = "USD"; 
            string dateFrom = "2024-01-01";
            string dateTo = "2024-01-05";

            var code = new IsoCode { Code = "USD" };

            var expectedData = new List<ExchangeRateDto>
            {
                new ExchangeRateDto { Rate = 1.2m, Amount = 1, ISO = code, Diff = 0.5m, RateDate = "2024-01-01" },
                new ExchangeRateDto { Rate = 1.5m, Amount = 1, ISO = code, Diff = 0.3m, RateDate = "2024-01-02", CBA_HasData = true},
                new ExchangeRateDto { Rate = 0, Amount = 0, ISO = code, Diff = 0, RateDate = "2024-01-02", CBA_HasData = false}
            };

            _client.Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedData);

            //Act

            var result = await _exchangeRateRepository.GetByRangeAsync(isoCode, dateFrom, dateTo);

            //Assert

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedData.Count, result.Count);
            for (int i = 0; i < expectedData.Count; i++)
            {
                Assert.AreEqual(expectedData[i].Rate, result[i].Rate);
                Assert.AreEqual(expectedData[i].Amount, result[i].Amount);
                Assert.AreEqual(expectedData[i].ISO.Code, result[i].ISO.Code);
                Assert.AreEqual(expectedData[i].Diff, result[i].Diff);
                Assert.AreEqual(expectedData[i].RateDate, result[i].RateDate);
                Assert.AreEqual(expectedData[i].CBA_HasData, result[i].CBA_HasData);
            }
        }
        
        [TestMethod]
        public async Task GetDataByRangeAsync_ThrowIfIsoCodeIsEmpty()
        {
            //Arrange
            string isoCode = "";
            string dateFrom = "2024-01-01";
            string dateTo = "2024-01-05";

            //Assert Act 

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _exchangeRateRepository.GetByRangeAsync(isoCode, dateFrom, dateTo);
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dataContext.Database.EnsureDeleted();
            _dataContext.Dispose();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}