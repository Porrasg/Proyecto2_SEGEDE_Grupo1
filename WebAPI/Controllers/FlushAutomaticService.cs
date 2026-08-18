using CoreApp;

namespace WebAPI.Controllers
{
    public class FlushAutomaticService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FlushAutomaticService> _logger;

        private DateTime _lastExecutionDate = DateTime.MinValue;

        public FlushAutomaticService(
            IServiceScopeFactory scopeFactory,
            ILogger<FlushAutomaticService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope =
                        _scopeFactory.CreateScope())
                    {
                        var configManager =
                            new FlushConfigManager();

                        var config =
                            configManager.RetrieveConfiguration();

                        // Si el proceso automático está desactivado,
                        // no hacemos nada.
                        if (config.IsAutomatic)
                        {
                            var now = DateTime.Now;

                            bool correctTime =
                                now.Hour == config.ExecutionTime.Hours &&
                                now.Minute == config.ExecutionTime.Minutes;

                            var flushManager = new FlushManager();
                            bool alreadyExecutedToday =
                                _lastExecutionDate.Date == now.Date ||
                                (correctTime && flushManager.HasAutomaticFlushForDate(now));

                            if (correctTime &&
                                !alreadyExecutedToday)
                            {
                                int processed =
                                    flushManager.ExecuteMassFlush(
                                        "Automatic"
                                    );

                                _lastExecutionDate = now;

                                _logger.LogInformation(
                                    "Flush automático ejecutado. Baterías procesadas: {Processed}",
                                    processed);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en Flush automático");
                }

                // Revisar cada 30 segundos
                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken
                );
            }
        }
    }
}
