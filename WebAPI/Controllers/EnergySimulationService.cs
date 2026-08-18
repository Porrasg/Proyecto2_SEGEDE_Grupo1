using CoreApp;
using Entities_DTOs;

namespace WebAPI.Controllers
{
    public class EnergySimulationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Cada cuánto se ejecutará la simulación.
        private readonly TimeSpan _interval =
            TimeSpan.FromMinutes(1);

        public EnergySimulationService(
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
                        var turbineManager =
                            new TurbineManager();

                        var energyManager =
                            new EnergyManager();

                        var turbines =
                            turbineManager.RetrieveAllTurbines();

                        foreach (var turbine in turbines)
                        {
                            if (turbine.Status == "Decommissioned")
                            {
                                continue;
                            }
                            // Como el servicio se ejecuta cada minuto, simulamos la energía correspondiente a ese intervalo.
                            decimal hours =
                                (decimal)_interval.TotalHours;

                            energyManager.ProcessTurbineEnergy(
                                turbine,
                                hours);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error en simulación de energía: {ex.Message}"
                    );
                }

                try
                {
                    await Task.Delay(
                        _interval,
                        stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}