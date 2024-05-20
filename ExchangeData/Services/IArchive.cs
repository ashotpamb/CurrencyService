using ExchangeData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Services
{
    public interface IArchive
    {
        Task<bool> AddDataAsync(string request, string response);
    }
}
