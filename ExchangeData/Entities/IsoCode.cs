using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Entities
{
    public class IsoCode
    {
        [Key]
        public long Id { get; set; }
        public string? Code { get; set; }

        public Collection<ExchangeRate> ExchangeRates { get; set; }
    }
}
