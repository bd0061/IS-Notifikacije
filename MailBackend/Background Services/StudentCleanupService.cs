namespace MailBackend.Background_Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MailBackend.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class StudentCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<StudentCleanupService> _logger;
        private readonly IServiceProvider _services;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _executingTask;

        public StudentCleanupService(IServiceProvider services, ILogger<StudentCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Pocetak cleanup servisa.");

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = Task.Run(() => DoWorkAsync(_cancellationTokenSource.Token), cancellationToken);

            return Task.CompletedTask;
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            do
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        
                        var now = DateTime.UtcNow;

                        var studentiBrisanje = await context.Studenti
                            .Where(s => !s.isVerified && s.RegisteredDate < now.AddDays(-1))
                            .ToListAsync();
                        context.Studenti.RemoveRange(studentiBrisanje);

                        await context.SaveChangesAsync();

                        _logger.LogInformation($"{studentiBrisanje.Count} nepotvrdjenih rekorda obrisano.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in DoWorkAsync.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Task was cancelled, exit the loop
                    break;
                }

            } while (!cancellationToken.IsCancellationRequested);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kraj cleanup servisa.");
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            if (_executingTask != null)
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
