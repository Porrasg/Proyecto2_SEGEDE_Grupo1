using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreApp
{
    // Lógica principal de facturas y estados de cuenta.
    public class InvoiceManager
    {

        // Devuelve todas las facturas registradas.
        public List<Invoice> RetrieveAllInvoices()
        {
            var crud = new InvoiceCrudFactory();

            return crud.RetrieveAll<Invoice>();
        }



        // Busca una factura por su identificador.
        public Invoice RetrieveById(int id)
        {
            // Validar el identificador de la factura
            if (id <= 0)
            {
                throw new Exception("El identificador de la factura no es válido");
            }

            var crud = new InvoiceCrudFactory();

            // Obtener la factura registrada
            var invoice = crud.RetrieveById<Invoice>(id);

            // Validar que la factura exista
            if (invoice == null)
            {
                throw new Exception("No se encontró la factura solicitada");
            }

            return invoice;
        }



        // Crea una nueva factura a partir de una distribución.
        public void Create(Invoice invoice)
        {
            // Validar que la factura no sea nula
            if (invoice == null)
            {
                throw new Exception("La factura no puede ser nula");
            }

            // Validar campos obligatorios
            if (HasEmptyFields(invoice))
            {
                throw new Exception("Todos los campos obligatorios deben completarse");
            }

            // Validar el porcentaje de impuesto
            if (invoice.TaxPercentage < 0)
            {
                throw new Exception("El porcentaje de impuesto no puede ser negativo");
            }

            var distributionCrud = new DistributionCrudFactory();

            // Obtener la distribución relacionada
            var distribution = distributionCrud.RetrieveById<Distribution>(invoice.DistributionId);

            // Validar que la distribución exista
            if (distribution == null)
            {
                throw new Exception("La distribución relacionada no existe");
            }

            // Validar que la distribución no esté cancelada
            if (distribution.Status == "Cancelled")
            {
                throw new Exception( "No se puede generar una factura para una distribución cancelada");
            }

            // Validar que la distribución tenga energía asignada
            if (distribution.AssignedEnergyMWh <= 0)
            {
                throw new Exception( "La distribución no posee energía asignada para facturar");
            }

            // Validar que el comprador corresponda con la distribución
            if (invoice.BuyerId != distribution.BuyerId)
            {
                throw new Exception("El comprador indicado no corresponde con la distribución");
            }

            var crud = new InvoiceCrudFactory();

            // Obtener todas las facturas registradas
            var invoices = crud.RetrieveAll<Invoice>();

            // Validar que la distribución no tenga una factura activa
            foreach (var existingInvoice in invoices)
            {
                if (existingInvoice.DistributionId == invoice.DistributionId &&
                    existingInvoice.PaymentStatus != "Cancelled")
                {
                    throw new Exception("La distribución ya posee una factura registrada");
                }
            }

            // Tomar los datos directamente de la distribución
            invoice.BuyerId = distribution.BuyerId;
            invoice.EnergyMWh = distribution.AssignedEnergyMWh;
            invoice.UnitPrice = distribution.UnitPrice;

            // Calcular el subtotal de la factura
            invoice.Subtotal = invoice.EnergyMWh * invoice.UnitPrice;

            // Calcular el monto del impuesto
            invoice.TaxAmount = invoice.Subtotal * (invoice.TaxPercentage / 100);

            // Calcular el monto total de la factura
            invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;

            // Validar que los montos calculados sean correctos
            if (HasInvalidAmounts(invoice))
            {
                throw new Exception("Los montos calculados de la factura no son válidos");
            }

            // Generar automáticamente el número de factura
            invoice.InvoiceNumber = GenerateInvoiceNumber();

            // Asignar la fecha de emisión
            invoice.IssueDate = DateTime.Now;

            // Asignar la fecha límite de pago
            invoice.DueDate = invoice.IssueDate.AddDays(30);

            // Asignar el estado inicial de pago
            invoice.PaymentStatus = "Pending";

            // Asignar la fecha de creación
            invoice.CreatedAt = DateTime.Now;

            // Crear la factura
            crud.Create(invoice);
        }

        // Actualiza solo el estado de pago u otros datos permitidos.
        public void Update(Invoice invoice)
        {
            // Validar que la factura no sea nula
            if (invoice == null)
            {
                throw new Exception("La factura no puede ser nula");
            }

            // Validar el identificador de la factura
            if (invoice.Id <= 0)
            {
                throw new Exception("El identificador de la factura no es válido");
            }

            // Validar el estado de pago
            if (!IsValidPaymentStatus(invoice.PaymentStatus))
            {
                throw new Exception("El estado de pago de la factura no es válido");
            }

            var crud = new InvoiceCrudFactory();

            // Obtener la factura registrada
            var currentInvoice = crud.RetrieveById<Invoice>(invoice.Id);

            // Validar que la factura exista
            if (currentInvoice == null)
            {
                throw new Exception("La factura que desea actualizar no existe");
            }

            // Validar que una factura cancelada no sea modificada
            if (currentInvoice.PaymentStatus == "Cancelled")
            {
                throw new Exception("No se puede modificar una factura cancelada");
            }

            // Validar que una factura pagada no sea modificada
            if (currentInvoice.PaymentStatus == "Paid")
            {
                throw new Exception("No se puede modificar una factura pagada");
            }

            // Evitar cancelar la factura desde Update
            if (invoice.PaymentStatus == "Cancelled")
            {
                throw new Exception("Para cancelar una factura debe utilizar el método de eliminación");
            }

            // Mantener la información original de la factura
            invoice.InvoiceNumber = currentInvoice.InvoiceNumber;
            invoice.DistributionId = currentInvoice.DistributionId;
            invoice.BuyerId = currentInvoice.BuyerId;
            invoice.IssueDate = currentInvoice.IssueDate;
            invoice.DueDate = currentInvoice.DueDate;
            invoice.EnergyMWh = currentInvoice.EnergyMWh;
            invoice.UnitPrice = currentInvoice.UnitPrice;
            invoice.Subtotal = currentInvoice.Subtotal;
            invoice.TaxPercentage = currentInvoice.TaxPercentage;
            invoice.TaxAmount = currentInvoice.TaxAmount;
            invoice.TotalAmount = currentInvoice.TotalAmount;
            invoice.CreatedAt = currentInvoice.CreatedAt;

            // Actualizar la factura
            crud.Update(invoice);
        }

        // Cancela la factura de forma lógica.
        public void Delete(Invoice invoice)
        {
            // Validar que la factura no sea nula
            if (invoice == null)
            {
                throw new Exception("La factura no puede ser nula");
            }

            // Validar el identificador de la factura
            if (invoice.Id <= 0)
            {
                throw new Exception("El identificador de la factura no es válido");
            }

            var crud = new InvoiceCrudFactory();

            // Obtener la factura registrada
            var currentInvoice = crud.RetrieveById<Invoice>(invoice.Id);

            // Validar que la factura exista
            if (currentInvoice == null)
            {
                throw new Exception("La factura que desea cancelar no existe");
            }

            // Validar que la factura no esté cancelada
            if (currentInvoice.PaymentStatus == "Cancelled")
            {
                throw new Exception( "La factura ya se encuentra cancelada");
            }

            // Validar que una factura pagada no sea cancelada
            if (currentInvoice.PaymentStatus == "Paid")
            {
                throw new Exception( "No se puede cancelar una factura pagada");
            }

            // El Stored Procedure cambia el estado a Cancelled
            crud.Delete(invoice);
        }


        // Revisa si faltan campos obligatorios.
        private bool HasEmptyFields(Invoice i)
        {
            return i.DistributionId <= 0 ||
                   i.BuyerId <= 0;
        }

        // Valida que los montos calculados de la factura sean correctos
        // Revisa si los montos calculados tienen sentido.
        private bool HasInvalidAmounts(Invoice i)
        {
            return i.EnergyMWh <= 0 ||
                   i.UnitPrice <= 0 ||
                   i.Subtotal <= 0 ||
                   i.TaxAmount < 0 ||
                   i.TotalAmount <= 0;
        }

        // Valida que el estado de pago sea uno de los permitidos
        // Valida los estados de pago que sí acepta el sistema.
        private bool IsValidPaymentStatus(string status)
        {
            return status == "Pending" ||
                   status == "Paid" ||
                   status == "Overdue" ||
                   status == "Cancelled";
        }

        // Genera un número de factura único basado en la fecha y hora actual
        // Crea un número de factura único y fácil de leer.
        private string GenerateInvoiceNumber()
        {
            return "INV-" +DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

    }
}