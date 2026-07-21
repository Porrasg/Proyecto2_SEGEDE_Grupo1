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
            var inv = baseDTO as Invoice;
            var sqlOperation = new SqlOperaton();
            sqlOperation.ProcedureName = "CRE_INVOICE_PR";

            sqlOperation.AddStringParameter("InvoiceNumber", inv.InvoiceNumber);
            sqlOperation.AddIntParameter("DistributionId", inv.DistributionId);
            sqlOperation.AddIntParameter("BuyerId", inv.BuyerId);
            sqlOperation.AddDateTimeParameter("IssueDate", inv.IssueDate);
            sqlOperation.AddDateTimeParameter("DueDate", inv.DueDate);
            sqlOperation.AddDecimalParameter("EnergyMWh", inv.EnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", inv.UnitPrice);
            sqlOperation.AddDecimalParameter("Subtotal", inv.Subtotal);
            sqlOperation.AddDecimalParameter("TaxPercentage", inv.TaxPercentage);
            sqlOperation.AddDecimalParameter("TaxAmount", inv.TaxAmount);
            sqlOperation.AddDecimalParameter("TotalAmount", inv.TotalAmount);
            sqlOperation.AddStringParameter("PaymentStatus", inv.PaymentStatus);
            sqlOperation.AddDateTimeParameter("CreatedAt", inv.CreatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var inv = baseDTO as Invoice;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_INVOICE_PR";

            sqlOperation.AddIntParameter("InvoiceId", inv.Id);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var op = new SqlOperation();
            op.ProcedureName = "RET_ALL_INVOICE_PR";
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var inv = BuildInvoice(row);
                    list.Add((T)Convert.ChangeType(inv, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation();
            op.ProcedureName = "RET_BY_ID_INVOICE_PR";
            op.AddIntParameter("InvoiceId", id);
            var results = sqlDao.ExecuteQueryProcedure(op);
            if (results.Count > 0)
            {
                var inv = BuildInvoice(results[0]);
                return (T)Convert.ChangeType(inv, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var inv = baseDTO as Invoice;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_INVOICE_PR";

            sqlOperation.AddIntParameter("InvoiceId", inv.Id);
            sqlOperation.AddStringParameter("InvoiceNumber", inv.InvoiceNumber);
            sqlOperation.AddIntParameter("DistributionId", inv.DistributionId);
            sqlOperation.AddIntParameter("BuyerId", inv.BuyerId);
            sqlOperation.AddDateTimeParameter("IssueDate", inv.IssueDate);
            sqlOperation.AddDateTimeParameter("DueDate", inv.DueDate);
            sqlOperation.AddDecimalParameter("EnergyMWh", inv.EnergyMWh);
            sqlOperation.AddDecimalParameter("UnitPrice", inv.UnitPrice);
            sqlOperation.AddDecimalParameter("Subtotal", inv.Subtotal);
            sqlOperation.AddDecimalParameter("TaxPercentage", inv.TaxPercentage);
            sqlOperation.AddDecimalParameter("TaxAmount", inv.TaxAmount);
            sqlOperation.AddDecimalParameter("TotalAmount", inv.TotalAmount);
            sqlOperation.AddStringParameter("PaymentStatus", inv.PaymentStatus);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private Invoice BuildInvoice(Dictionary<string, object> row)
        {
            var inv = new Invoice
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
            return inv;
        }
    }
}
