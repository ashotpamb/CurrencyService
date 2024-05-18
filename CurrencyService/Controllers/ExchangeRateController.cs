
using ExchangeData.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CurrencyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : Controller
    {
        private readonly IExchangeRateRepository _exchangeRateRepository;

        public ExchangeRateController(IExchangeRateRepository exchangeRateRepository)
        {
            _exchangeRateRepository = exchangeRateRepository;
        }


        /// <summary>
        /// Gets the exchange rates by date range and ISO codes.
        /// </summary>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="ISOCode"></param>
        [HttpGet]
        public async Task<ActionResult> ExchangeRatesByDate(
            [FromQuery(Name = "DateFrom")] string DateFrom,
            [FromQuery(Name = "DateTo")] string DateTo,
            [FromQuery] string ISOCode)
        {
            try
            {
                var response = await _exchangeRateRepository.GetAsync(5);
                return Ok(response);

            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
        }

    }
}
