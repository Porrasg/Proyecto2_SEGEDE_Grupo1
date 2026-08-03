using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace CoreApp
{
    // Lógica de negocio de la definición maestra de precios e impuestos (Admin/Prices, Admin/Taxes)
    public class BillingManager
    {
        public void SetPrice(decimal priceCRCPerMWh)
        {
            if (priceCRCPerMWh <= 0)
            {
                throw new Exception("El precio por MWh debe ser mayor a cero");
            }

            var crud = new PriceCrudFactory();
            crud.Create(new Price { PriceCRCPerMWh = priceCRCPerMWh });
        }

        public List<Price> RetrievePriceHistory()
        {
            var crud = new PriceCrudFactory();
            return crud.RetrieveAll<Price>();
        }

        public Price RetrieveActivePrice()
        {
            var crud = new PriceCrudFactory();
            return crud.RetrieveActive();
        }

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

        public List<Tax> RetrieveTaxHistory()
        {
            var crud = new TaxCrudFactory();
            return crud.RetrieveAll<Tax>();
        }

        public Tax RetrieveActiveTax()
        {
            var crud = new TaxCrudFactory();
            return crud.RetrieveActive();
        }
    }
}
