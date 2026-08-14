using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreApp
{
    // Lógica de negocio para repartir energía y generar facturas.
    public class DistributionManager
    {
        // Devuelve todas las distribuciones guardadas.
        public List<Distribution> RetrieveAllDistributions()
        {
            var crud = new DistributionCrudFactory();
            return crud.RetrieveAll<Distribution>();
        }

        // Ejecuta el cierre mensual del día 30.
        // El cierre toma la energia disponible en el Banco Central,
        // distribuye los forecasts pendientes y genera las facturas correspondientes.
        public List<Distribution> ExecuteMonthlyClosing(
            int year,
            int month,
            int centralBankId)
        {
            if (year <= 0)
            {
                throw new Exception("El año indicado no es valido");
            }

            if (month < 1 || month > 12)
            {
                throw new Exception("El mes indicado no es valido");
            }

            var lastDay = DateTime.DaysInMonth(year, month);
            var closingDate = new DateTime(year, month, lastDay);
          //  if (DateTime.Now.Date != closingDate.Date)
          //  {
          //      throw new Exception(
          //          "El cierre mensual solo puede ejecutarse el último día del mes");
         //   }
           
            // Ejecutar la distribucion mensual.
            return ExecuteMonthlyDistribution(
                year,
                month,
                centralBankId
            );
        }

        // Hace el cierre mensual y reparte la energía entre los forecasts pendientes.
        // Si alcanza para todos, asigna completo, si no reparte proporcionalmente.
        public List<Distribution> ExecuteMonthlyDistribution(
            int year,
            int month,
            int centralBankId)
            {
            var forecastManager = new ForecastManager();

            // Obtener los forecasts pendientes del periodo.
            var pendingForecasts = forecastManager
                .RetrieveByPeriod(year, month)
                .Where(f => f.Status == "Pending")
                .ToList();

            if (!pendingForecasts.Any())
            {
                throw new Exception(
                    "No hay solicitudes de compra pendientes para distribuir en el periodo indicado");
            }

            // Obtener el Banco Central.
            var centralBankCrud = new CentralBankCrudFactory();

            var centralBank = centralBankCrud
                .RetrieveById<CentralBank>(centralBankId);

            if (centralBank == null)
            {
                throw new Exception(
                    "El banco central indicado no existe");
            }

            // Validar que el Banco Central esté activo.
            if (centralBank.Status != "Active")
            {
                throw new Exception(
                    "El banco central debe encontrarse activo");
            }

            // Obtener precio vigente.
            var billingManager = new BillingManager();

            var activePrice = billingManager.RetrieveActivePrice();

            if (activePrice == null)
            {
                throw new Exception(
                    "No hay un precio por MWh vigente configurado. " +
                    "Configure uno en Administración > Precios antes de ejecutar la distribución.");
            }

            // Obtener impuesto vigente.
            var activeTax = billingManager.RetrieveActiveTax();

            // Calcular la demanda total.
            decimal totalDemand = pendingForecasts
                .Sum(f => f.RequestedEnergyMWh);

            // Energía disponible en el Banco Central.
            decimal availableEnergy =
                centralBank.CurrentInventoryMWh;

            if (totalDemand <= 0)
            {
                throw new Exception(
                    "La demanda total debe ser mayor a cero");
            }

            if (availableEnergy < 0)
            {
                throw new Exception(
                    "El inventario del banco central no puede ser negativo");
            }

            // Calcular el porcentaje de prorrateo
            // Ejemplos:
            // 100% -> hay suficiente energía.
            // 90%  -> solo se puede cubrir el 90%.
            // 80%  -> solo se puede cubrir el 80%.
            decimal ratio = CalculateProrationRatio(
                totalDemand,
                availableEnergy);

            // Si existe demanda pero no alcanza ni para el 10%, no se ejecuta la distribución.
            Console.WriteLine($"TOTAL DEMANDA: {totalDemand}");
            Console.WriteLine($"ENERGIA DISPONIBLE: {availableEnergy}");


            if (ratio <= 0)
            {
                throw new Exception(
                    "La energía disponible no permite cubrir ni el 10% de la demanda");
            }

            // Obtener el siguiente número de lote.
            var crud = new DistributionCrudFactory();

            var existingBatches =
                crud.RetrieveAll<Distribution>();

            var newBatchId =
                existingBatches.Any()
                    ? existingBatches.Max(d => d.DistributionBatchId) + 1
                    : 1;

            decimal totalAssigned = 0;

            // Crear una distribución para cada forecast.
            foreach (var forecast in pendingForecasts)
            {
                // Calcular la energía asignada según el porcentaje
                // de prorrateo seleccionado.
                decimal assigned =
                    Math.Round(
                        forecast.RequestedEnergyMWh * ratio,
                        4);

                // Calcular la energía que quedó sin asignar.
                decimal unassigned =
                    Math.Round(
                        forecast.RequestedEnergyMWh - assigned,
                        4);

                var distribution = new Distribution
                {
                    DistributionBatchId = newBatchId,

                    ForecastId = forecast.Id,

                    BuyerId = forecast.BuyerId,

                    CentralBankId = centralBankId,

                    RequestedEnergyMWh =
                        forecast.RequestedEnergyMWh,

                    AssignedEnergyMWh =
                        assigned,

                    UnassignedEnergyMWh =
                        unassigned,

                    UnitPrice =
                        activePrice.PriceCRCPerMWh,

                    DistributionDate =
                        DateTime.Now,

                    CreatedAt =
                        DateTime.Now
                };

                // Create() se encarga de determinar Status = Completed o Partial.
                Create(distribution);

                totalAssigned += assigned;

                // Marcar el forecast como procesado para que no vuelva a participar en otro cierre.
                try
                {
                    forecastManager.MarkAsProcessed(
                        forecast.Id);
                }
                catch
                {
                    // La distribución ya fue creada.
                }
            }

            // Descontar del Banco Central únicamente la energía que realmente fue asignada.
            if (totalAssigned > 0)
            {
                new CentralBankManager()
                    .DistributeEnergy(
                        centralBankId,
                        totalAssigned);
            }

            // Recuperar las distribuciones recién creadas para obtener sus IDs generados por la BD.
            var createdDistributions =
                crud.RetrieveByBatchId(newBatchId);

            // Generar una factura para cada distribución.
            foreach (var distribution in createdDistributions)
            {
                try
                {
                    new InvoiceManager().Create(
                        new Invoice
                        {
                            DistributionId =
                                distribution.Id,

                            BuyerId =
                                distribution.BuyerId,

                            TaxPercentage =
                                (activeTax?.Percentage ?? 0) * 100
                        });
                }
                catch
                {
                    // La distribución no se revierte
                    // si ocurre un problema al generar la factura.
                }
            }

            return createdDistributions;
        }


        // Crea una distribución manual o desde el cierre mensual.
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
                    var notificationManager = new NotificationManager();
                    notificationManager.SendEnergyQuotaNotification(
                        buyer.Email,
                        buyer.FirstName,
                        $"{distribution.AssignedEnergyMWh} MWh",
                        distribution.DistributionDate.ToString("dd/MM/yyyy")
                    );
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


        // Actualiza una distribución ya registrada.
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

        // Cancela una distribución sin borrarla físicamente.
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

        // Busca una distribución por su identificador.
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


        // Devuelve todas las distribuciones de un mismo lote.
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


        // Devuelve las distribuciones asociadas a un forecast.
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


        // Devuelve las distribuciones de un comprador.
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


        // Devuelve las distribuciones de un banco central.
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


        // Devuelve las distribuciones que tengan un estado específico.
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


        // Devuelve las distribuciones dentro de un rango de fechas.
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

        // Calcula el porcentaje de energía que se puede asignar de forma uniforme.
        // Si no existe suficiente energía, reduce la demanda en escalones de 10%, 90%, 80%, 70%, etc.
        private decimal CalculateProrationRatio(
            decimal totalDemand,
            decimal availableEnergy)
        {
            if (totalDemand <= 0)
            {
                return 0m;
            }

            // Si existe suficiente energía, se asigna el 100%.
            if (availableEnergy >= totalDemand)
            {
                return 1m;
            }

            // Porcentaje real disponible.
            decimal availableRatio = availableEnergy / totalDemand;

            // Convertir el porcentaje a un escalón de 10%.
            decimal ratio = Math.Floor(availableRatio * 10m) / 10m;

            // Nunca permitir un porcentaje menor que 0%.
            if (ratio < 0m)
            {
                ratio = 0m;
            }

            return ratio;
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
