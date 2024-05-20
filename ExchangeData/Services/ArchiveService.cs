using ExchangeData.Data;
using ExchangeData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeData.Services
{
    public class ArchiveService : IArchive
    {
        private readonly ILogger<ArchiveService> _logger;
        private readonly DataContext _dataContext;

        public ArchiveService(ILogger<ArchiveService> logger, DataContext dataContext)
        {
            _logger = logger;
            _dataContext = dataContext;
        }
        public async Task<bool> AddDataAsync(string request, string response)
        {
            using (var transaction = await _dataContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    var archive = new Archive() { Request = request, Response = response };
                    _dataContext.Archives.Add(archive);

                    await _dataContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(ex, "Error occurred while adding data to the archive: {ErrorMessage}", ex.Message);
                    return false;
                }
            }
        }
    }
}
