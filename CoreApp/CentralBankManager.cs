using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class CentralBankManager
    {

        public List<CentralBank> RetrieveAllCentralBanks()
        {
            var crud = new CentralBankCrudFactory();
            return crud.RetrieveAll<CentralBank>();
        }

        public CentralBank RetrieveById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El identificador del banco central no es válido");
            }

            var centralBankCrud = new CentralBankCrudFactory();

            // Obtener el banco central registrado
            var centralBank = centralBankCrud.RetrieveById<CentralBank>(id);

            // Validar que el banco central exista
            if (centralBank == null)
            {
                throw new Exception("No se encontró el banco central solicitado");
            }

            return centralBank;
        }

        public void Create(CentralBank centralBank)
        {
            if (centralBank == null)
            {
                throw new Exception("El banco central no puede ser nulo");
            }

            // Asignar el estado inicial
            centralBank.Status = "Active";

            if (HasEmptyFields(centralBank))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar la capacidad máxima
            if (centralBank.MaximumCapacityMWh <= 0)
            {
                centralBank.MaximumCapacityMWh = CalculateDefaultCapacity();

                if (centralBank.MaximumCapacityMWh <= 0)
                {
                    throw new Exception("No hay turbinas activas para poder realizar el calculo");
                }
            }

            var centralBankCrud = new CentralBankCrudFactory();

            // Obtener los bancos centrales registrados
            var centralBanks = centralBankCrud.RetrieveAll<CentralBank>();

            // Validar que no exista otro banco con el mismo nombre
            foreach (var currentCentralBank in centralBanks)
            {
                if (currentCentralBank.Name.Trim().ToLower() ==
                    centralBank.Name.Trim().ToLower())
                {
                    throw new Exception("Ya existe un banco central con el nombre indicado");
                }
            }

            // Asignar los valores iniciales
            centralBank.CurrentInventoryMWh = 0;
            centralBank.TotalReceivedMWh = 0;
            centralBank.TotalDistributedMWh = 0;
            centralBank.TotalSaturationLossMWh = 0;
            centralBank.CreatedAt = DateTime.Now;
            centralBank.UpdatedAt = null;

            // Crear el banco central
            centralBankCrud.Create(centralBank);
        }

        public void Update(CentralBank centralBank)
        {
            if (centralBank == null)
            {
                throw new Exception("El banco central no puede ser nulo");
            }

            if (centralBank.Id <= 0)
            {
                throw new Exception("El identificador del banco central no es válido");
            }

            if (HasEmptyFields(centralBank))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar la capacidad máxima
            if (centralBank.MaximumCapacityMWh <= 0)
            {
                throw new Exception("La capacidad máxima debe ser mayor a cero");
            }

            // Validar el estado
            if (!IsValidStatus(centralBank.Status))
            {
                throw new Exception("El estado del banco central no es válido");
            }

            var centralBankCrud = new CentralBankCrudFactory();

            // Obtener el banco central registrado
            var currentCentralBank = centralBankCrud.RetrieveById<CentralBank>(centralBank.Id);

            // Validar que el banco central exista
            if (currentCentralBank == null)
            {
                throw new Exception("El banco central que desea actualizar no existe");
            }

            // Validar que el banco central no esté inactivo
            if (currentCentralBank.Status == "Inactive")
            {
                throw new Exception("No se puede modificar un banco central inactivo");
            }

            // Validar que la nueva capacidad soporte el inventario actual
            if (centralBank.MaximumCapacityMWh <
                currentCentralBank.CurrentInventoryMWh)
            {
                throw new Exception("La capacidad máxima no puede ser menor que el inventario actual del banco central");
            }

            // Obtener todos los bancos centrales
            var centralBanks = centralBankCrud.RetrieveAll<CentralBank>();

            // Validar que no exista otro banco con el mismo nombre
            foreach (var current in centralBanks)
            {
                if (current.Id != centralBank.Id &&
                    current.Name.Trim().ToLower() ==
                    centralBank.Name.Trim().ToLower())
                {
                    throw new Exception("Ya existe otro banco central con el nombre indicado");
                }
            }

            // Mantener los valores energéticos actuales
            centralBank.CurrentInventoryMWh = currentCentralBank.CurrentInventoryMWh;
            centralBank.TotalReceivedMWh = currentCentralBank.TotalReceivedMWh;
            centralBank.TotalDistributedMWh = currentCentralBank.TotalDistributedMWh;
            centralBank.TotalSaturationLossMWh = currentCentralBank.TotalSaturationLossMWh;

            // Mantener la fecha original de creación
            centralBank.CreatedAt = currentCentralBank.CreatedAt;

            // Asignar la fecha de actualización
            centralBank.UpdatedAt = DateTime.Now;

            // Actualizar el banco central
            centralBankCrud.Update(centralBank);
        }


        public void Delete(CentralBank centralBank)
        {
            if (centralBank == null)
            {
                throw new Exception("El banco central no puede ser nulo");
            }

            if (centralBank.Id <= 0)
            {
                throw new Exception("El identificador del banco central no es válido");
            }

            var centralBankCrud = new CentralBankCrudFactory();

            // Obtener el banco central registrado
            var currentCentralBank = centralBankCrud.RetrieveById<CentralBank>(centralBank.Id);

            // Validar que el banco central exista
            if (currentCentralBank == null)
            {
                throw new Exception("El banco central que desea desactivar no existe");
            }

            // Validar que no se encuentre inactivo
            if (currentCentralBank.Status == "Inactive")
            {
                throw new Exception("El banco central ya se encuentra inactivo");
            }

            // Validar que no contenga energía
            if (currentCentralBank.CurrentInventoryMWh > 0)
            {
                throw new Exception("No se puede desactivar un banco central que todavía contiene energía");
            }

            // Asignar el estado inactivo
            centralBank.Status = "Inactive";

            // Asignar la fecha de actualización
            centralBank.UpdatedAt = DateTime.Now;

            // Desactivar lógicamente el banco central
            centralBankCrud.Delete(centralBank);
        }

        private bool HasEmptyFields(
            CentralBank centralBank)
        {
            return string.IsNullOrWhiteSpace(
                       centralBank.Name) ||
                   string.IsNullOrWhiteSpace(
                       centralBank.Status);
        }

        private bool IsValidStatus(string status)
        {
            return status == "Active" ||
                   status == "Inactive";
        }

        //metodo para calcular la capacidad maxima de acuerdo a las turbianas activas
        private decimal CalculateDefaultCapacity()
        {
            var turbineCrud = new TurbineCrudFactory();
            var activeTurbines = turbineCrud.RetrieveByStatus("Active");
            decimal totalCapacity = activeTurbines.Sum(t => t.NominalWeeklyCapacityMWh);

            return totalCapacity;
        }

        // Método para actualizar el maximo cada vez que una turbina cambie de estado
        public void UpdateMaximumCapacity()
        {
            var centralBankCrud = new CentralBankCrudFactory();

            // se obtiene el Banco Central de la BD
            var centralBank = centralBankCrud.RetrieveAll<CentralBank>().FirstOrDefault();

            //  si existe
            if (centralBank != null)
            {
                // Recalcula la capacidad basandonos en las turbinas activas
                centralBank.MaximumCapacityMWh = CalculateDefaultCapacity();
                centralBank.UpdatedAt = DateTime.Now;

                //  Actualizamos solo si el objeto no es nulo
                centralBankCrud.Update(centralBank);
            }
            else
            {
                throw new Exception("No se encontró el registro del Banco Central.");
            }
        }

        // entrada de energia
        public void ReceiveEnergy(int centralBankId, decimal mwhToreceive)
        {
            if (mwhToreceive <= 0)
            {
                throw new Exception("La cantidad de energia a recibir debe ser mayor a cero");
            }
            var centralBankCrud = new TurbineCrudFactory();
            var centralBank = centralBankCrud.RetrieveById<CentralBank>(centralBankId);

            if (centralBank == null)
            {
                throw new Exception("El banco central no existe");
            }
            //se acumula el total recibido
            centralBank.TotalReceivedMWh = mwhToreceive;

            //se calcula el espacio disponible
            decimal availableSpace = centralBank.MaximumCapacityMWh - centralBank.CurrentInventoryMWh;

            //se calcula se cabe o hay saturacion 
            if (mwhToreceive <= availableSpace)
            {
                //cabe toda la energia
                centralBank.CurrentInventoryMWh += mwhToreceive;
            }
            else
            {
                //se llena el inventario maximo
                centralBank.CurrentInventoryMWh = centralBank.MaximumCapacityMWh;
                //el exceso para a la saturacion
                decimal overflow = mwhToreceive - availableSpace;
                centralBank.TotalSaturationLossMWh += overflow;

            }
            centralBank.UpdatedAt = DateTime.UtcNow;
            centralBankCrud.Update(centralBank);
        }
        //salida de energia
        public void DistributeEnergy(int centralBankId, decimal mwhToDistribute)
        {
            //validar que la cantidad de energia a distribuir sea positica
            if (mwhToDistribute <= 0)
            {
                throw new Exception("La cantidad de energia a distribuir debe ser mayor a cero");
            }
            var centralBankCrud = new CentralBankCrudFactory();
            var centralBank = centralBankCrud.RetrieveById<CentralBank>(centralBankId);
            if (centralBank == null)
            {
                throw new Exception("El banco centrol no existe");
            }
            //se valida que haya sufiente energia para distribuir
            if (mwhToDistribute > centralBank.CurrentInventoryMWh)
            {
                throw new Exception("No hay suficiente energia en el inventario para realizar la distribucion");
            }
            // se descuenta del inventario y acumla en lo distribuido
            centralBank.CurrentInventoryMWh -= mwhToDistribute;
            centralBank.TotalDistributedMWh += mwhToDistribute;

            centralBankCrud.Update(centralBank);
            centralBank.UpdatedAt=DateTime.UtcNow;
        }
    }
}


