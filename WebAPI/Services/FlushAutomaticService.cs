using CoreApp;

namespace WebAPI.Services
{
    public class FlushAutomaticService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private DateTime _lastExecutionDate = DateTime.MinValue;

        public FlushAutomaticService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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

                            bool alreadyExecutedToday =
                                _lastExecutionDate.Date == now.Date;

                            if (correctTime &&
                                !alreadyExecutedToday)
                            {
                                var flushManager =
                                    new FlushManager();

                                int processed =
                                    flushManager.ExecuteMassFlush(
                                        "Automatic"
                                    );

                                _lastExecutionDate = now;

                                Console.WriteLine(
                                    $"Flush automático ejecutado. " +
                                    $"Baterías procesadas: {processed}"
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error en Flush automático: {ex.Message}"
                    );
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