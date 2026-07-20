using System;
using System.Collections.Generic;
using System.Text;

namespace Entities_DTOs
{
    // Almacena el historial de las distribuciones de energía realizadas a un comprador
    public class Distribution : BaseDTO
    {
        public int DistributionBatchId { get; set; } // distribuciones realizadas durante una misma ejecución

        public int ForecastId { get; set; } // identifica la solicitud que originó la distribución

        public int BuyerId { get; set; } // comprador que recibió la energía

        public int CentralBankId { get; set; } // indica desde que banco central se realizó la distribución

        public decimal RequestedEnergyMWh { get; set; } // copia de la energía solicitada en el forecast

        public decimal AssignedEnergyMWh { get; set; } // cantidad de energía entregada al comprador

        public decimal UnassignedEnergyMWh { get; set; } // energía que no pudo entregarse

        public decimal UnitPrice { get; set; } // precio unitario de la energía entregada al comprador

        public DateTime DistributionDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
