using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace CoreApp
{
    // Lógica de negocio para manejar precios e impuestos desde administración.
    public class BillingManager
    {
        // Guarda un precio nuevo por MWh.
        public void SetPrice(decimal priceCRCPerMWh)
        {
            if (priceCRCPerMWh <= 0)
            {
                throw new Exception("El precio por MWh debe ser mayor a cero");
            }

            var crud = new PriceCrudFactory();
            crud.Create(new Price { PriceCRCPerMWh = priceCRCPerMWh });
        }

        // Trae el historial completo de precios.
        public List<Price> RetrievePriceHistory()
        {
            var crud = new PriceCrudFactory();
            return crud.RetrieveAll<Price>();
        }

        // Trae el precio que está activo en este momento.
        public Price RetrieveActivePrice()
        {
            var crud = new PriceCrudFactory();
            return crud.RetrieveActive();
        }

        // Guarda un nuevo impuesto activo.
        public void SetTax(string name, decimal percentage)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("El nombre del impuesto es requerido");
            }

            if (percentage < 0 || percentage >= 1)
            {
                throw new Exception("El porcentaje del impuesto debe estar entre 0% y 100%");
            }

            var crud = new TaxCrudFactory();
            crud.Create(new Tax { Name = name.Trim(), Percentage = percentage });
        }

        // Trae el historial completo de impuestos.
        public List<Tax> RetrieveTaxHistory()
        {
            var crud = new TaxCrudFactory();
            return crud.RetrieveAll<Tax>();
        }

        // Trae el impuesto que está activo en este momento.
        public Tax RetrieveActiveTax()
        {
            var crud = new TaxCrudFactory();
            return crud.RetrieveActive();
        }
    }
}
