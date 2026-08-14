using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class CentralBankMovementManager
    {
        public List<CentralBankMovement> RetrieveAll()
        {
            var crud = new CentralBankMovementCrudFactory();
            return crud.RetrieveAll<CentralBankMovement>();
        }
        public CentralBankMovement RetrieveById(int id)
        {
            if (id <= 0)
            {
                throw new Exception("El identificador del movimiento no es válido.");
            }

            var crud = new CentralBankMovementCrudFactory();
            var movement = crud.RetrieveById<CentralBankMovement>(id);

            if (movement == null)
            {
                throw new Exception("El movimiento no existe.");
            }

            return movement;
        }

        public void Create(CentralBankMovement movement)
        {
            if (movement == null)
            {
                throw new Exception("La información del movimiento es inválida.");
            }

            ValidateMovement(movement);

            movement.CreatedAt = DateTime.UtcNow;
            movement.UpdatedAt = null;

            var crud = new CentralBankMovementCrudFactory();
            crud.Create(movement);
        }
        public void Update(CentralBankMovement movement)
        {
            if (movement == null)
            {
                throw new Exception("La información del movimiento es inválida.");
            }

            if (movement.Id <= 0)
            {
                throw new Exception("El identificador del movimiento no es válido.");
            }

            ValidateMovement(movement);

            movement.UpdatedAt = DateTime.UtcNow;

            var crud = new CentralBankMovementCrudFactory();
            crud.Update(movement);
        }

        public void Delete(CentralBankMovement movement)
        {
            if (movement == null)
            {
                throw new Exception("La información del movimiento es inválida.");
            }

            if (movement.Id <= 0)
            {
                throw new Exception("El identificador del movimiento no es válido.");
            }

            movement.UpdatedAt = DateTime.UtcNow;

            var crud = new CentralBankMovementCrudFactory();
            crud.Delete(movement);
        }

        private void ValidateMovement(CentralBankMovement movement)
        {
            if (movement.CentralBankId <= 0)
            {
                throw new Exception("El banco central no es válido.");
            }

            if (movement.EnergyMWh <= 0)
            {
                throw new Exception("La energía debe ser mayor que cero.");
            }

            if (movement.InventoryAfterMovement < 0)
            {
                throw new Exception("El inventario no puede ser negativo.");
            }

            if (movement.SaturationLossMWh < 0)
            {
                throw new Exception("La pérdida por saturación no puede ser negativa.");
            }

            if (string.IsNullOrWhiteSpace(movement.Description))
            {
                throw new Exception("Debe indicar una descripción del movimiento.");
            }

            if (!IsValidMovementType(movement.MovementType))
            {
                throw new Exception("El tipo de movimiento no es válido.");
            }
        }

        private bool IsValidMovementType(string type)
        {
            return type == "RECEIVE" ||
                   type == "DISTRIBUTE" ||
                   type == "FLUSH";
        }
    }
}
