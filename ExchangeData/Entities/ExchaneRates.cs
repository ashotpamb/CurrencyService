using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Entities
{
    public class ExchaneRates
    {
        public long ID { get; set; }
        public decimal Rate { get; set; }
        public int Amount { get; set; }
        public string ISO { get; set; }
        public decimal Diff { get; set; }

        private DateTime RateDate { get; set; }
    }
}
