using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    public class InvoiceManager
    {

        public List<Invoice> RetrieveAllInvoices()
        {
            var crud = new InvoiceCrudFactory();

            return crud.RetrieveAll<Invoice>();
        }



        public Invoice RetrieveById(int id)
        {
            var crud = new InvoiceCrudFactory();

            return crud.RetrieveById<Invoice>(id);
        }



        public void Create(Invoice invoice)
        {

            if (HasEmptyFields(invoice))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }


            if (invoice.DueDate < invoice.IssueDate)
            {
                throw new Exception("La fecha límite no puede ser anterior a la emisión");
            }


            if (HasInvalidAmounts(invoice))
            {
                throw new Exception("Los montos de la factura deben ser válidos");
            }


            if (invoice.TaxPercentage < 0)
            {
                throw new Exception("El impuesto no puede ser negativo");
            }


            var crud = new InvoiceCrudFactory();

            crud.Create(invoice);
        }

        public void Update(Invoice invoice)
        {
            var crud = new InvoiceCrudFactory();

            crud.Update(invoice);
        }

        public void Delete(Invoice invoice)
        {
            var crud = new InvoiceCrudFactory();

            crud.Delete(invoice);
        }
        private bool HasEmptyFields(Invoice i)
        {
            return string.IsNullOrWhiteSpace(i.InvoiceNumber) ||
                   string.IsNullOrWhiteSpace(i.PaymentStatus) ||
                   i.DistributionId <= 0 ||
                   i.BuyerId <= 0;
        }
        private bool HasInvalidAmounts(Invoice i)
        {
            return i.EnergyMWh <= 0 ||
                   i.UnitPrice <= 0 ||
                   i.Subtotal < 0 ||
                   i.TaxAmount < 0 ||
                   i.TotalAmount <= 0;
        }

    }
}