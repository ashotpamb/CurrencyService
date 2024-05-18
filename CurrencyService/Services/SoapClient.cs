using CurrencyService.Services;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Unicode;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Xml;

namespace CurrencyService.Domain
{
    public class SoapClient : IClient
    {
        private readonly HttpClient _httpClient;
        private const string _URL = "https://api.cba.am/exchangerates.asmx";
        private string _ACTION = "http://www.cba.am/";


        public SoapClient()
        {
            _httpClient = new HttpClient();
        }
        public async Task<List<ExchangeRate>> PostAsync(string iso, string from, string to)
        {
            string inputDateFormat = "yyyy-MM-dd";

            if (!DateTime.TryParseExact(from, inputDateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime dateFrom) ||
                !DateTime.TryParseExact(to, inputDateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime dateTo))
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

            if (diffgramNode is null) throw _= new Exception("Invalid ISO code");

            XmlNodeList exchangeRatesNodes = diffgramNode.SelectNodes("ExchangeRatesByRange");

            List<ExchangeRate> exchangeRates = new();

            foreach (XmlNode node in exchangeRatesNodes)
            {
                exchangeRates.Add(DeserializeXml<ExchangeRate>(node.OuterXml));
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
