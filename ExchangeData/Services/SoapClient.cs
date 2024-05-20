
using System.Text;

using System.Xml.Serialization;
using System.Xml;
using ExchangeData.Services;
using ExchangeDate.Domain;
using ExchangeData.Dtos;
using System.Globalization;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExchangeData.Services
{
    public class SoapClient : IClient
    {
        private readonly HttpClient _httpClient;
        private readonly IArchive _acrchive;
        private const string _URL = "https://api.cba.am/exchangerates.asmx";
        private string _ACTION = "http://www.cba.am/";


        public SoapClient(IArchive acrchive)
        {
            _httpClient = new HttpClient();
            _acrchive = acrchive;
        }
        public async Task<List<ExchangeRateDto>> PostAsync(string iso, string from, string to)
        {
            ArgumentNullException.ThrowIfNull(iso);

            if (!DateTime.TryParse(from, out DateTime dateFrom) ||
                !DateTime.TryParse(to,out DateTime dateTo))
            {
                throw new ArgumentException("Invalid date format. Please use 'yyyy-MM-dd'.");
            }

            string isoDateFrom = dateFrom.ToString("s");
            string isoDateTo = dateTo.ToString("s");

            var requestEnvelope = ExchangeRatesByDate(iso, isoDateFrom, isoDateTo);
            var content = new StringContent(requestEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", _ACTION + "ExchangeRatesByDateRangeByISO");

            var response = await _httpClient.PostAsync(_URL, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(responseContent);
            XmlNode diffgramNode = xmlDocument.SelectSingleNode("//diffgr:diffgram/DocumentElement", GetNamespaceManager(xmlDocument));

            if (diffgramNode is null) throw _= new Exception($"Data with ISO Code: {iso} not found");

            //Add data to the archive asynchronously
            _ = Task.Run(async () =>
            {
                _ = await _acrchive.AddDataAsync(await content.ReadAsStringAsync(), responseContent);
            });

            XmlNodeList exchangeRatesNodes = diffgramNode.SelectNodes("ExchangeRatesByRange");

            var allData = Enumerable.Range(0, (dateTo - dateFrom).Days + 1)
             .Select(offset => dateFrom.AddDays(offset))
             .ToList();

            List<ExchangeRateDto> exchangeRates = ProcessData(exchangeRatesNodes);

            var missingData = GetMissingData(allData, exchangeRates);

            return exchangeRates.Concat(missingData).OrderBy(e => e.RateDate).ToList();
        }

        /// <summary>
        /// Get missing data if client returned empty for some date
        /// </summary>
        /// <param name="exchangeRateDtos"></param>
        /// <returns>Returns Missing data parsed ExchangeRateDto to object</returns>
        private List<ExchangeRateDto> GetMissingData(List<DateTime> dateTimes, List<ExchangeRateDto> exchangeRateDtos)
        {
            var soapData = exchangeRateDtos.Select(e => e.RateDate).ToHashSet();
            var dateTostring = dateTimes.Select(t => t.ToString("yyyy-MM-dd")).ToHashSet();

            var missingDates = dateTostring
                .Where(date => !soapData.Contains(date))
                .ToList();

            var missingData = missingDates.Select(date => new ExchangeRateDto
            {
                Rate = 0,
                Amount = 0,
                ISO = null,
                Diff = 0,
                RateDate = date,
            }).ToList();

            return missingData;
        }

        private List<ExchangeRateDto> ProcessData(XmlNodeList exchangeRatesNodes)
        {
            List<ExchangeRateDto> exchangeRates = new List<ExchangeRateDto>();

            foreach (XmlNode node in exchangeRatesNodes)
            {
                var tempObj = DeserializeXml<ExchangeRate>(node.OuterXml);

                ExchangeRateDto exchangeRateDto = new ExchangeRateDto
                {
                    Rate = tempObj.Rate,
                    Amount = tempObj.Amount,
                    ISO = tempObj.ISO,
                    Diff = tempObj.Diff,
                    RateDate = tempObj.RateDate,
                    CBA_HasData = true,
                };
                exchangeRates.Add(exchangeRateDto);
            }


            return exchangeRates;
        }

        private T DeserializeXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        private XmlNamespaceManager GetNamespaceManager(XmlDocument xmlDoc)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("diffgr", "urn:schemas-microsoft-com:xml-diffgram-v1");
            return nsManager;
        }
        private string ExchangeRatesByDate(string iso, string from, string to)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                <ExchangeRatesByDateRangeByISO xmlns=""http://www.cba.am/"">
                <ISOCodes>{iso}</ISOCodes>
                <DateFrom>{from}</DateFrom>
                <DateTo>{to}</DateTo>
                </ExchangeRatesByDateRangeByISO>
                </soap:Body>
                </soap:Envelope>";
        }

    }
}
