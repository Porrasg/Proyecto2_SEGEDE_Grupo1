using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class FlushManager
    {
        public List<Flush> RetrieveAllFlushes()
        {
            var flushCrud = new FlushCrudFactory();
            return flushCrud.RetrieveAll<Flush>();
        }
        public Flush RetrieveById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El identificador del vaciado no es válido");
            }

            var flushCrud = new FlushCrudFactory();

            // Obtener el vaciado registrado
            var flush = flushCrud.RetrieveById<Flush>(id);

            if (flush == null)
            {
                throw new Exception("No se encontró el vaciado solicitado");
            }

            return flush;
        }


        public void Create(Flush flush)
        {
            if (flush == null)
            {
                throw new Exception("El vaciado no puede ser nulo");
            }

            // Asignar el estado inicial
            flush.Status = "Completed";

            if (HasEmptyFields(flush))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar el tipo de ejecución
            if (!IsValidExecutionType(flush.ExecutionType))
            {
                throw new Exception("El tipo de ejecución del vaciado no es válido");
            }

            // Validar el estado
            if (!IsValidStatus(flush.Status))
            {
                throw new Exception("El estado del vaciado no es válido");
            }

            // Validar la fecha de ejecución
            if (flush.ExecutedAt == default(DateTime))
            {
                throw new Exception("La fecha de ejecución del vaciado es obligatoria");
            }

            // Validar que la fecha no sea futura
            if (flush.ExecutedAt > DateTime.Now)
            {
                throw new Exception("La fecha de ejecución del vaciado no puede ser futura");
            }

            var turbineCrud = new TurbineCrudFactory();

            // Obtener la turbina asociada
            var turbine = turbineCrud.RetrieveById<Turbine>(flush.TurbineId);

            // Validar que la turbina exista
            if (turbine == null)
            {
                throw new Exception("La turbina seleccionada no existe");
            }

            // Validar que la turbina no esté dada de baja
            if (turbine.Status == "Decommissioned")
            {
                throw new Exception("No se puede realizar un vaciado desde una turbina dada de baja");
            }

            var batteryCrud = new BatteriesCrudFactory();

            // Obtener la batería asociada
            var battery = batteryCrud.RetrieveById<Battery>(flush.BatteryId);

            // Validar que la batería exista
            if (battery == null)
            {
                throw new Exception("La batería seleccionada no existe");
            }

            // Validar que la batería pertenezca a la turbina
            if (battery.TurbineId != flush.TurbineId)
            {
                throw new Exception("La batería seleccionada no pertenece a la turbina indicada");
            }

            // Validar que la batería se encuentre activa
            if (battery.Status != "Active")
            {
                throw new Exception("La batería seleccionada no se encuentra activa");
            }

            // Asignar las fechas iniciales
            flush.SnapshotEnergyMWh = 0;
            flush.TransferredEnergyMWh = 0;
            flush.SaturationLossMWh = 0;
            flush.Status = "Completed";
            flush.CreatedAt = DateTime.Now;
            flush.UpdatedAt = null;

            var flushCrud = new FlushCrudFactory();

            // Crear el registro del vaciado
            flushCrud.Create(flush);
        }


        public void Update(Flush flush)
        {
            if (flush == null)
            {
                throw new Exception("El vaciado no puede ser nulo");
            }

            if (flush.Id <= 0)
            {
                throw new Exception("El identificador del vaciado no es válido");
            }

            if (HasEmptyFields(flush))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar los valores de energía
            if (HasNegativeEnergy(flush))
            {
                throw new Exception("Los valores de energía no pueden ser negativos");
            }

            // Validar el tipo de ejecución
            if (!IsValidExecutionType(flush.ExecutionType))
            {
                throw new Exception("El tipo de ejecución del vaciado no es válido");
            }

            if (!IsValidStatus(flush.Status))
            {
                throw new Exception("El estado del vaciado no es válido");
            }

            // Validar la distribución de energía
            if (flush.TransferredEnergyMWh +
                flush.SaturationLossMWh !=
                flush.SnapshotEnergyMWh)
            {
                throw new Exception("La energía transferida y la pérdida por saturación deben coincidir con la energía disponible antes del vaciado");
            }

            var flushCrud = new FlushCrudFactory();

            // Obtener el vaciado registrado
            var currentFlush = flushCrud.RetrieveById<Flush>(flush.Id);

            // Validar que el vaciado exista
            if (currentFlush == null)
            {
                throw new Exception("El vaciado que desea actualizar no existe");
            }

            // Validar que el vaciado no esté cancelado
            if (currentFlush.Status == "Cancelled")
            {
                throw new Exception("No se puede modificar un vaciado cancelado");
            }

            // Mantener la fecha de creación original
            flush.CreatedAt = currentFlush.CreatedAt;

            // Asignar la fecha de actualización
            flush.UpdatedAt = DateTime.Now;

            // Actualizar el vaciado
            flushCrud.Update(flush);
        }


        public void Delete(Flush flush)
        {
            if (flush == null)
            {
                throw new Exception("El vaciado no puede ser nulo");
            }

            if (flush.Id <= 0)
            {
                throw new Exception("El identificador del vaciado no es válido");
            }

            var flushCrud = new FlushCrudFactory();

            // Obtener el vaciado registrado
            var currentFlush = flushCrud.RetrieveById<Flush>(flush.Id);

            // Validar que el vaciado exista
            if (currentFlush == null)
            {
                throw new Exception("El vaciado que desea cancelar no existe");
            }

            // Validar que no se encuentre cancelado
            if (currentFlush.Status == "Cancelled")
            {
                throw new Exception("El vaciado ya se encuentra cancelado");
            }

            // Asignar el estado de cancelación
            flush.Status = "Cancelled";

            // Asignar la fecha de actualización
            flush.UpdatedAt = DateTime.Now;

            // Cancelar lógicamente el vaciado
            flushCrud.Delete(flush);
        }


        private bool HasEmptyFields(Flush flush)
        {
            return flush.FlushBatchId <= 0 ||
                   flush.TurbineId <= 0 ||
                   flush.BatteryId <= 0 ||
                   flush.CentralBankId <= 0 ||
                   string.IsNullOrWhiteSpace(flush.ExecutionType) ||
                   string.IsNullOrWhiteSpace(flush.Status);
        }

        private bool HasNegativeEnergy(Flush flush)
        {
            return flush.SnapshotEnergyMWh < 0 ||
                   flush.TransferredEnergyMWh < 0 ||
                   flush.SaturationLossMWh < 0;
        }

        private bool IsValidExecutionType(
            string executionType)
        {
            return executionType == "Automatic" ||
                   executionType == "Manual";
        }

        private bool IsValidStatus(string status)
        {
            return status == "Completed" ||
                   status == "Cancelled";
        }
 

    //metodo encargado del vaciado al bancocentral
    public void ExecuteFlush(Flush flush)
        {
            if (flush == null)
            {
                throw new ArgumentNullException("La informacion del vaciado es invalida");
            }
            if (flush.TransferredEnergyMWh <= 0)
            {
                throw new Exception("La energia a transferir debe ser mayor a cero");
            }

            var batteryCrud = new BatteriesCrudFactory();
            var battery = batteryCrud.RetrieveById<Battery>(flush.BatteryId);

            if (battery == null)
            {
                throw new Exception("La bateria no existe.");
            }
            if(battery.Status !="Active")
            {
                throw new Exception("Para realizar un vaciado la bateria debe estar activa");
            }
            if (battery.CurrentEnergyMWh < flush.TransferredEnergyMWh)
            {
                throw new Exception("La bateria no tiene suficiente energia");
            }
            flush.SnapshotEnergyMWh = battery.CurrentEnergyMWh; // Guardar la energia actual de la bateria antes del vaciado

            var centralManager = new CentralBankManager();

            decimal loss = centralManager.ReceiveEnergy(flush.CentralBankId, flush.TransferredEnergyMWh);

            flush.SaturationLossMWh = loss;
            // Actualizar la bateria
            battery.CurrentEnergyMWh -= flush.TransferredEnergyMWh;
            battery.TotalTransferredMWh += flush.TransferredEnergyMWh;
            battery.UpdatedAt = DateTime.UtcNow;

            batteryCrud.Update(battery);

            //consultar el bance central para hacer la actualizacion de la energia y registrar el movimiento
            var centralBankCrud = new CentralBankCrudFactory();
            var centralBank = centralBankCrud.RetrieveById<CentralBank>(flush.CentralBankId);

            //registrar el movimiento en el banco central
            var movementManager = new CentralBankMovementManager();

            movementManager.Create(new CentralBankMovement
            {
                CentralBankId = flush.CentralBankId,
                MovementType = "FLUSH",
                EnergyMWh = flush.TransferredEnergyMWh,
                InventoryAfterMovement = centralBank.CurrentInventoryMWh,
                SaturationLossMWh = flush.SaturationLossMWh,
                Description = $"Vaciado de {flush.TransferredEnergyMWh} MWh desde la bateria {flush.BatteryId} de la turbina {flush.TurbineId}"

            });
            //registrar el vaciado 
            flush.ExecutedAt = DateTime.UtcNow;
            flush.CreatedAt = DateTime.UtcNow;

            flush.Status="Completed";
            var flushCrud = new FlushCrudFactory();
            flushCrud.Create(flush);
        }
    }
}
