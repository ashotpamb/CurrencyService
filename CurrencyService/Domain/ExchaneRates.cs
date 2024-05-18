using System.Xml.Serialization;

namespace CurrencyService.Domain
{
    [XmlRoot("ExchangeRatesByRange")]
    public class ExchangeRate
    {

        public decimal Rate { get; set; }
        public int Amount { get; set; }
        public string ISO { get; set; }
        public decimal Diff { get; set; }

        private DateTime rateDate;

        public string RateDate
        {
            get { return rateDate.ToString("yyyy-MM-dd"); }
            set { rateDate = DateTime.Parse(value); }
        }
    }

}
