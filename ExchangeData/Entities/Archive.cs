using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Entities
{
    public class Archive
    {
        public Archive() 
        { 
            CreatedAt = DateTime.UtcNow;
        }

        [Key]
        public int ID { get; set; }
        public string Request {  get; set; }

        public string Response { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
