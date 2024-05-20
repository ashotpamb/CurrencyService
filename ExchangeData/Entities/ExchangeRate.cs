using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Entities
{
    public class ExchangeRate
    {
        [Key]
        public long ID { get; set; }
        public decimal? Rate { get; set; }
        public int? Amount { get; set; }

        [ForeignKey(nameof(CodeID))]
        public long? CodeID { get; set; }
        public IsoCode? ISO { get; set; }
        public decimal? Diff { get; set; }
        public DateTime RateDate { get; set; }

        public bool CBA_HasData { get; set; } = false;
    }
}
