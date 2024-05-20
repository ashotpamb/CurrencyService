using System.Xml.Serialization;

namespace ExchangeDate.Domain
{
    [XmlRoot("ExchangeRatesByRange")]
    public class ExchangeRate : IDisposable
    {

        public decimal Rate { get; set; }
        public int Amount { get; set; }
        public string ISO { get; set; }
        public decimal Diff { get; set; }
        public string? RateDate { get; set; }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}
