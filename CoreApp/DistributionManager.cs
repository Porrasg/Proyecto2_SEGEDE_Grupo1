using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreApp
{
    public class DistributionManager
    {
        public List<Distribution> RetrieveAllDistributions()
        {
            var crud = new DistributionCrudFactory();
            return crud.RetrieveAll<Distribution>();
        }

        // Cierre mensual: distribuye la energía disponible en el Banco Central entre todas
        // las solicitudes de compra (Forecast) Pendientes del periodo. Si el inventario
        // disponible alcanza para toda la demanda, cada comprador recibe el 100% de lo
        // solicitado. Si no alcanza, se aplica un prorrateo uniforme (la misma fracción del
        // inventario disponible para todos) - matemáticamente reproduce los ejemplos de la
        // rúbrica (90%/80%/70%...) cuando el disponible es exactamente esa fracción de la
        // demanda total, sin inventar quiebres discrecionales.
        public List<Distribution> ExecuteMonthlyDistribution(int year, int month, int centralBankId)
        {
            var forecastManager = new ForecastManager();
            var pendingForecasts = forecastManager.RetrieveByPeriod(year, month)
                .Where(f => f.Status == "Pending")
                .ToList();

            if (!pendingForecasts.Any())
            {
                throw new Exception("No hay solicitudes de compra pendientes para distribuir en el periodo indicado");
            }

            var centralBankCrud = new CentralBankCrudFactory();
            var centralBank = centralBankCrud.RetrieveById<CentralBank>(centralBankId);

            if (centralBank == null)
            {
                throw new Exception("El banco central indicado no existe");
            }

            var billingManager = new BillingManager();
            var activePrice = billingManager.RetrieveActivePrice();

            if (activePrice == null)
            {
                throw new Exception("No hay un precio por MWh vigente configurado. Configure uno en Administración > Precios antes de ejecutar la distribución.");
            }

            var activeTax = billingManager.RetrieveActiveTax();

            var totalDemand = pendingForecasts.Sum(f => f.RequestedEnergyMWh);
            var available = centralBank.CurrentInventoryMWh;

            // Prorrateo uniforme: 100% si alcanza, o la misma fracción del disponible para todos si no alcanza
            var ratio = (totalDemand <= 0 || totalDemand <= available) ? 1m : available / totalDemand;

            var crud = new DistributionCrudFactory();
            var existingBatches = crud.RetrieveAll<Distribution>();
            var newBatchId = existingBatches.Any() ? existingBatches.Max(d => d.DistributionBatchId) + 1 : 1;

            decimal totalAssigned = 0;

            foreach (var forecast in pendingForecasts)
            {
                var assigned = Math.Round(forecast.RequestedEnergyMWh * ratio, 4);

                var distribution = new Distribution
                {
                    DistributionBatchId = newBatchId,
                    ForecastId = forecast.Id,
                    BuyerId = forecast.BuyerId,
                    CentralBankId = centralBankId,
                    RequestedEnergyMWh = forecast.RequestedEnergyMWh,
                    AssignedEnergyMWh = assigned,
                    UnitPrice = activePrice.PriceCRCPerMWh
                };

                Create(distribution);

                totalAssigned += assigned;

                // El forecast pasa a Processed independientemente de si se marca como
                // "Pending" bloqueado en el futuro - no vuelve a considerarse en otra ejecución.
                try { forecastManager.MarkAsProcessed(forecast.Id); }
                catch { /* no revertir la distribución ya creada por un fallo aquí */ }
            }

            // Descontar del banco central el total efectivamente asignado en este lote
            if (totalAssigned > 0)
            {
                new CentralBankManager().DistributeEnergy(centralBankId, totalAssigned);
            }

            // Recuperar las distribuciones recién creadas (con su Id real generado por el SP,
            // que Create() no puede devolver) para generar la factura de cada una y notificar.
            var createdDistributions = crud.RetrieveByBatchId(newBatchId);

            foreach (var distribution in createdDistributions)
            {
                try
                {
                    new InvoiceManager().Create(new Invoice
                    {
                        DistributionId = distribution.Id,
                        BuyerId = distribution.BuyerId,
                        TaxPercentage = (activeTax?.Percentage ?? 0) * 100 // InvoiceManager espera el porcentaje en base 100
                    });
                }
                catch { /* no revertir la distribución ya creada por un fallo al facturar */ }
            }

            return createdDistributions;
        }

        public void Create(Distribution distribution)
        {
            // Validar que la distribución no sea nula
            if (distribution == null)
            {
                throw new Exception("La distribución no puede ser nula");
            }

            // Validar campos obligatorios
            if (HasEmptyFields(distribution))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar valores negativos
            if (HasNegativeEnergy(distribution))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }

            // Validar la energía solicitada
            if (distribution.RequestedEnergyMWh <= 0)
            {
                throw new Exception("La energía solicitada debe ser mayor a cero");
            }

            // Validar que la energía asignada no supere la solicitada
            if (distribution.AssignedEnergyMWh >
                distribution.RequestedEnergyMWh)
            {
                throw new Exception("La energía asignada no puede superar la solicitada");
            }

            // Validar el precio unitario
            if (distribution.UnitPrice <= 0)
            {
                throw new Exception("El precio unitario debe ser mayor a cero");
            }

            var forecastCrud = new ForecastCrudFactory();

            // Obtener el forecast relacionado
            var forecast = forecastCrud.RetrieveById<Forecast>(distribution.ForecastId);

            // Validar que exista
            if (forecast == null)
            {
                throw new Exception("El forecast relacionado no existe");
            }

            // Validar que no esté cancelado
            if (forecast.Status == "Cancelled")
            {
                throw new Exception("No se puede distribuir energía para un forecast cancelado");
            }

            // Validar que no haya sido procesado
            if (forecast.Status == "Processed")
            {
                throw new Exception("El forecast ya fue procesado");
            }

            // Validar que el comprador corresponda al forecast
            if (forecast.BuyerId != distribution.BuyerId)
            {
                throw new Exception("El comprador indicado no corresponde al forecast");
            }

            var centralBankCrud = new CentralBankCrudFactory();

            // Obtener el banco central
            var centralBank = centralBankCrud.RetrieveById<CentralBank>(distribution.CentralBankId);

            // Validar que exista
            if (centralBank == null)
            {
                throw new Exception("El banco central indicado no existe");
            }

            // Validar que esté activo
            if (centralBank.Status != "Active")
            {
                throw new Exception("El banco central debe encontrarse activo");
            }

            // Asignar valores desde el forecast
            distribution.BuyerId = forecast.BuyerId;

            distribution.RequestedEnergyMWh = forecast.RequestedEnergyMWh;

            // Validar inventario disponible
            if (distribution.AssignedEnergyMWh > centralBank.CurrentInventoryMWh)
            {
                throw new Exception("El banco central no posee suficiente energía disponible");
            }

            // Calcular energía no asignada
            distribution.UnassignedEnergyMWh = distribution.RequestedEnergyMWh - distribution.AssignedEnergyMWh;

            // Asignar el estado
            if (distribution.AssignedEnergyMWh == distribution.RequestedEnergyMWh)
            {
                distribution.Status = "Completed";
            }
            else
            {
                distribution.Status = "Partial";
            }

            // Asignar fechas
            distribution.DistributionDate = DateTime.Now;

            distribution.CreatedAt = DateTime.Now;

            var crud = new DistributionCrudFactory();

            // Crear la distribución
            crud.Create(distribution);

            // Notificar al comprador la cuota de energía asignada (correo + registro en la app).
            // Cada canal en su propio try/catch para no revertir la distribución ya creada.
            var userCrud = new UserCrudFactory();
            var buyer = userCrud.RetrieveById<User>(distribution.BuyerId);

            if (buyer != null)
            {
                try
                {
                    new OtpManager().SendGenericEmail(
                        buyer.Email,
                        buyer.FirstName,
                        "Cuota de energía asignada",
                        $"Se le asignó una cuota de <strong>{distribution.AssignedEnergyMWh} MWh</strong> " +
                        $"({distribution.DistributionDate:dd/MM/yyyy}). Puede consultar el detalle en su Estado de Cuenta.");
                }
                catch { /* no bloquear la distribución ya creada */ }

                try
                {
                    new NotificationManager().Create(new Notification
                    {
                        UserId = buyer.Id,
                        Title = "Cuota de energía asignada",
                        Message = $"{distribution.AssignedEnergyMWh} MWh asignados el {distribution.DistributionDate:dd/MM/yyyy}.",
                        NotificationType = "Distribution"
                    });
                }
                catch { /* no bloquear la distribución ya creada */ }
            }
        }


        public void Update(Distribution distribution)
        {
            // Validar que la distribución no sea nula
            if (distribution == null)
            {
                throw new Exception("La distribución no puede ser nula");
            }

            // Validar el identificador
            if (distribution.Id <= 0)
            {
                throw new Exception("El identificador de la distribución no es válido");
            }

            // Validar el estado
            if (!IsValidStatus(distribution.Status))
            {
                throw new Exception("El estado de la distribución no es válido");
            }

            var crud = new DistributionCrudFactory();

            // Obtener la distribución actual
            var currentDistribution = crud.RetrieveById<Distribution>(distribution.Id);

            // Validar que exista
            if (currentDistribution == null)
            {
                throw new Exception("La distribución que desea actualizar no existe");
            }

            // Validar que no esté cancelada
            if (currentDistribution.Status ==
                "Cancelled")
            {
                throw new Exception("No se puede modificar una distribución cancelada");
            }

            // Mantener los datos históricos
            distribution.DistributionBatchId = currentDistribution.DistributionBatchId;
            distribution.ForecastId = currentDistribution.ForecastId;
            distribution.BuyerId = currentDistribution.BuyerId;
            distribution.CentralBankId = currentDistribution.CentralBankId;
            distribution.RequestedEnergyMWh = currentDistribution.RequestedEnergyMWh;
            distribution.AssignedEnergyMWh = currentDistribution.AssignedEnergyMWh;
            distribution.UnassignedEnergyMWh = currentDistribution.UnassignedEnergyMWh;
            distribution.UnitPrice = currentDistribution.UnitPrice;
            distribution.DistributionDate = currentDistribution.DistributionDate;
            distribution.CreatedAt = currentDistribution.CreatedAt;

            // Actualizar la distribución
            crud.Update(distribution);
        }


        public void Delete(Distribution distribution)
        {
            // Validar que la distribución no sea nula
            if (distribution == null)
            {
                throw new Exception("La distribución no puede ser nula");
            }

            // Validar el identificador
            if (distribution.Id <= 0)
            {
                throw new Exception("El identificador de la distribución no es válido");
            }

            var crud = new DistributionCrudFactory();

            // Obtener la distribución actual
            var currentDistribution = crud.RetrieveById<Distribution>(distribution.Id);

            // Validar que exista
            if (currentDistribution == null)
            {
                throw new Exception("La distribución que desea cancelar no existe");
            }

            // Validar que no esté cancelada
            if (currentDistribution.Status ==
                "Cancelled")
            {
                throw new Exception("La distribución ya se encuentra cancelada");
            }

            // Asignar eliminación lógica
            distribution.Status = "Cancelled";

            // Cancelar la distribución
            crud.Delete(distribution);
        }

        public Distribution RetrieveById(int id)
        {
            // Validar el identificador de la distribución
            if (id <= 0)
            {
                throw new Exception( "El identificador de la distribución no es válido");
            }

            var crud = new DistributionCrudFactory();

            // Obtener la distribución
            var distribution = crud.RetrieveById<Distribution>(id);

            // Validar que exista
            if (distribution == null)
            {
                throw new Exception("No se encontró la distribución solicitada");
            }

            return distribution;
        }


        public List<Distribution> RetrieveByBatchId(int distributionBatchId)
        {
            // Validar el identificador del lote
            if (distributionBatchId <= 0)
            {
                throw new Exception("El identificador del lote no es válido");
            }

            var crud = new DistributionCrudFactory();

            return crud.RetrieveByBatchId(distributionBatchId);
        }


        public List<Distribution> RetrieveByForecastId(int forecastId)
        {
            // Validar el identificador del forecast
            if (forecastId <= 0)
            {
                throw new Exception("El identificador del forecast no es válido");
            }

            var crud = new DistributionCrudFactory();

            return crud.RetrieveByForecastId(forecastId);
        }


        public List<Distribution> RetrieveByBuyerId(int buyerId)
        {
            // Validar el identificador del comprador
            if (buyerId <= 0)
            {
                throw new Exception("El identificador del comprador no es válido");
            }

            var crud = new DistributionCrudFactory();

            return crud.RetrieveByBuyerId(buyerId);
        }


        public List<Distribution> RetrieveByCentralBankId(int centralBankId)
        {
            // Validar el identificador del banco central
            if (centralBankId <= 0)
            {
                throw new Exception( "El identificador del banco central no es válido");
            }

            var crud = new DistributionCrudFactory();

            return crud.RetrieveByCentralBankId(centralBankId);
        }


        public List<Distribution> RetrieveByStatus(string status)
        {
            // Validar el estado
            if (!IsValidStatus(status))
            {
                throw new Exception("El estado de la distribución no es válido");
            }

            var crud = new DistributionCrudFactory();

            return crud.RetrieveByStatus(status);
        }


        public List<Distribution> RetrieveByDateRange(DateTime startDate, DateTime endDate)
        {
            // Validar las fechas
            if (startDate == default || endDate == default)
            {
                throw new Exception("Las fechas del rango son obligatorias");
            }

            // Validar el orden de las fechas
            if (startDate > endDate)
            {
                throw new Exception("La fecha inicial no puede ser posterior a la fecha final");
            }

            var crud = new DistributionCrudFactory();

            return crud.RetrieveByDateRange(startDate, endDate);
        }

        private bool HasEmptyFields(
            Distribution distribution)
        {
            return distribution.DistributionBatchId <= 0 ||
                   distribution.ForecastId <= 0 ||
                   distribution.BuyerId <= 0 ||
                   distribution.CentralBankId <= 0;
        }

        private bool HasNegativeEnergy(
            Distribution distribution)
        {
            return distribution.RequestedEnergyMWh < 0 ||
                   distribution.AssignedEnergyMWh < 0 ||
                   distribution.UnassignedEnergyMWh < 0;
        }

        private bool IsValidStatus(string status)
        {
            return status == "Completed" ||
                   status == "Partial" ||
                   status == "Cancelled";
        }
    }
}
