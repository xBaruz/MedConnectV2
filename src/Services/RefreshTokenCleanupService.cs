using MedConnect.Repositories;

namespace MedConnect.Services;

public class RefreshTokenCleanupService(
    IServiceProvider serviceProvider, 
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RefreshToken Cleanup Service został uruchomiony.");

        using var timer = new PeriodicTimer(CheckInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var writeRepository = scope.ServiceProvider.GetRequiredService<IUserWriteRepository>();

                logger.LogInformation("Rozpoczęto automatyczne usuwanie wygasłych Refresh Tokenów...");
                await writeRepository.RemoveExpiredRefreshTokensAsync(stoppingToken);
                logger.LogInformation("Usuwanie wygasłych Refresh Tokenów zakończone sukcesem.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Wystąpił błąd podczas usuwania wygasłych Refresh Tokenów.");
            }
        }
    }
}