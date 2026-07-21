using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class InvoiceCrudFactory : CrudFactory
    {
        public InvoiceCrudFactory() {
            sqlDao = SqlDao.GetInstance();

        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto Invoice
            var invoice = baseDTO as Invoice;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_INVOICE_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("InvoiceNumber", invoice.InvoiceNumber);
            sqlOperation.AddIntParameter("DistributionId", invoice.DistributionId);
            sqlOperation.AddIntParameter("BuyerId", invoice.BuyerId);
            sqlOperation.AddDateTimeParameter("IssueDate", invoice.IssueDate);
            sqlOperation.AddDateTimeParameter("DueDate", invoice.DueDate);
            sqlOperation.AddDecimalParameter("EnergyMWh", invoice.EnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", invoice.UnitPrice);
            sqlOperation.AddDecimalParameter("Subtotal", invoice.Subtotal);
            sqlOperation.AddDecimalParameter("TaxPercentage", invoice.TaxPercentage);
            sqlOperation.AddDecimalParameter("TaxAmount", invoice.TaxAmount);
            sqlOperation.AddDecimalParameter("TotalAmount", invoice.TotalAmount);
            sqlOperation.AddStringParameter("PaymentStatus", invoice.PaymentStatus);
            sqlOperation.AddDateTimeParameter("CreatedAt", invoice.CreatedAt);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Invoice
            var invoice = baseDTO as Invoice;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_INVOICE_PR";

            sqlOperation.AddIntParameter("InvoiceId", invoice.Id);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todas las facturas que se obtengan de la consulta a la BD
            var lsInvoices = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_INVOICE_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto Invoice, luego agregarlo a la lista de facturas
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var invoice = BuildInvoice(row);
                    lsInvoices.Add((T)Convert.ChangeType(invoice, typeof(T)));
                }
            }
            return lsInvoices;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_INVOICE_PR";

            sqlOperation.AddIntParameter("InvoiceId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto Invoice y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var invoice = BuildInvoice(lsResults[0]);
                return (T)Convert.ChangeType(invoice, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto Invoice
            var invoice = baseDTO as Invoice;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_INVOICE_PR";

            sqlOperation.AddIntParameter("InvoiceId", invoice.Id);
            sqlOperation.AddStringParameter("InvoiceNumber", invoice.InvoiceNumber);
            sqlOperation.AddIntParameter("DistributionId", invoice.DistributionId);
            sqlOperation.AddIntParameter("BuyerId", invoice.BuyerId);
            sqlOperation.AddDateTimeParameter("IssueDate", invoice.IssueDate);
            sqlOperation.AddDateTimeParameter("DueDate", invoice.DueDate);
            sqlOperation.AddDecimalParameter("EnergyMWh", invoice.EnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", invoice.UnitPrice);
            sqlOperation.AddDecimalParameter("Subtotal", invoice.Subtotal);
            sqlOperation.AddDecimalParameter("TaxPercentage", invoice.TaxPercentage);
            sqlOperation.AddDecimalParameter("TaxAmount", invoice.TaxAmount);
            sqlOperation.AddDecimalParameter("TotalAmount", invoice.TotalAmount);
            sqlOperation.AddStringParameter("PaymentStatus", invoice.PaymentStatus);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Metodo que construye un objeto Invoice a partir a partir de la data que viene en la consulta de la BD
        private Invoice BuildInvoice(Dictionary<string, object> row)
        {
            var invoice = new Invoice
            {
                Id = (int)row["InvoiceId"],
                InvoiceNumber = (string)row["InvoiceNumber"],
                DistributionId = (int)row["DistributionId"],
                BuyerId = (int)row["BuyerId"],
                IssueDate = (DateTime)row["IssueDate"],
                DueDate = (DateTime)row["DueDate"],
                EnergyMWh = (decimal)row["EnergyMWh"],
                UnitPrice = (decimal)row["UnitPrice"],
                Subtotal = (decimal)row["Subtotal"],
                TaxPercentage = (decimal)row["TaxPercentage"],
                TaxAmount = (decimal)row["TaxAmount"],
                TotalAmount = (decimal)row["TotalAmount"],
                PaymentStatus = (string)row["PaymentStatus"],
                CreatedAt = (DateTime)row["CreatedAt"]
            };
            return invoice;
        }
    }
}
