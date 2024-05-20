using ExchangeData.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveCleanupService
{
    public class ArchiveCleanupWorker : BackgroundService
    {
        private readonly ILogger<ArchiveCleanupWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ArchiveCleanupWorker(ILogger<ArchiveCleanupWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Archive cleanup service is running.");

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                        var cutoffDate = DateTime.UtcNow.AddMonths(-1);
                        var archivedData = await dataContext.Archives
                            .Where(a => a.CreatedAt < cutoffDate).ToListAsync();

                        dataContext.Archives.RemoveRange(archivedData);
                        await dataContext.SaveChangesAsync();
                    }

                    _logger.LogInformation("Archive cleanup completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during archive cleanup: {ErrorMessage}", ex.Message);
                }
                await Task.Delay(TimeSpan.FromDays(30), stoppingToken);
            }
        }
    }
}
