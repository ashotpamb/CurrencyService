using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData
{
    public class ExchangeRateException : Exception
    {
        public ExchangeRateException() { }
        public ExchangeRateException(string message) :base(message) { }
    }
}
