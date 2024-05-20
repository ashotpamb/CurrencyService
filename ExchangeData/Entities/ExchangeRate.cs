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
        public override bool Equals(object? obj)
        {
            if (obj is ExchangeRate other)
            {
                return Rate == other.Rate &&
                       Amount == other.Amount &&
                       Diff == other.Diff &&
                       RateDate == other.RateDate &&
                       CBA_HasData == other.CBA_HasData;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Rate.GetHashCode();
                hash = hash * 23 + Amount.GetHashCode();
                hash = hash * 23 + Diff.GetHashCode();
                hash = hash * 23 + RateDate.GetHashCode();
                hash = hash * 23 + CBA_HasData.GetHashCode();
                return hash;
            }
        }
    }
}
