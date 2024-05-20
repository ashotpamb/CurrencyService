using ExchangeData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Dtos
{
    public class ExchangeRateDto
    {
        public decimal Rate { get; set; }
        public int Amount { get; set; }
        public IsoCode ISO { get; set; }
        public decimal Diff { get; set; }

        private string _rateDate;
        public string RateDate
        {
            get
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(_rateDate);
                return dateTimeOffset.ToString("yyyy-MM-dd");
            }
            set
            {
                _rateDate = value;
            }
        }

        public bool CBA_HasData { get;  set; }

        public object Serialize()
        {
            if (Rate == default || Amount == default || Diff == default)
            {
                return new { Message = $"CBA has no data for Date: { RateDate }", CBA_HasData };
            }
            else
            {
                return new { Rate, Amount, ISO.Code, Diff, RateDate, CBA_HasData, Message = "Success" };
            }
        }
    }
}
