using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace CoreApp
{
    public class ProductionCutManager
    {
        // Ejecuta el corte de producción de todas las baterías activas.
        public void ExecuteProductionCut(int centralBankId)
        {
            if (centralBankId <= 0)
            {
                throw new Exception(
                    "El identificador del banco central no es válido");
            }

            var batteryCrud = new BatteriesCrudFactory();

            // Obtener todas las baterías registradas.
            var batteries = batteryCrud.RetrieveAll<Battery>();

            if (batteries == null || batteries.Count == 0)
            {
                throw new Exception(
                    "No existen baterías registradas para ejecutar el corte");
            }

            var centralBankManager = new CentralBankManager();

            decimal totalTransferred = 0;
            decimal totalOverflow = 0;

            foreach (var battery in batteries)
            {
                // Solo participan las baterías activas.
                if (battery.Status != "Active")
                {
                    continue;
                }

                // Si la batería no tiene energía, no hay nada que transferir.
                if (battery.CurrentEnergyMWh <= 0)
                {
                    continue;
                }

                decimal energyToTransfer =
                    battery.CurrentEnergyMWh;

                // Enviar la energía al Banco Central.
                decimal overflow =
                    centralBankManager.ReceiveEnergy(
                        centralBankId,
                        energyToTransfer);

                // La energía que realmente ingresó al banco.
                decimal transferred =
                    energyToTransfer - overflow;

                // Acumular los totales del corte.
                totalTransferred += transferred;
                totalOverflow += overflow;

                // La energía ya salió de la batería.
                battery.CurrentEnergyMWh = 0;

                // El acumulado de producción corresponde al período que acaba de finalizar, por lo que se reinicia.
                battery.TotalGeneratedMWh = 0;

                // Las pérdidas del período también se reinician.
                battery.TotalSaturationLossMWh = 0;

                // El total transferido sí permanece como histórico.
                battery.TotalTransferredMWh += transferred;

                battery.UpdatedAt = DateTime.Now;

                // Guardar los cambios de la batería.
                batteryCrud.Update(battery);
            }

            if (totalTransferred == 0 && totalOverflow == 0)
            {
                throw new Exception(
                    "No hay energía disponible en las baterías para ejecutar el corte");
            }
        }
    }
}