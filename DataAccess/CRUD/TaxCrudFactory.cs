using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    // Mismo patrón inmutable que PriceCrudFactory: solo alta, el impuesto vigente
    // anterior se cierra automáticamente al registrar uno nuevo (ver CRE_TAX_PR).
    public class TaxCrudFactory : CrudFactory
    {
        public TaxCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var tax = baseDTO as Tax;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_TAX_PR";

            sqlOperation.AddStringParameter("Name", tax.Name);
            sqlOperation.AddDecimalParameter("Percentage", tax.Percentage);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Update(BaseDTO baseDTO)
        {
            throw new Exception("El historial de impuestos no puede modificarse");
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new Exception("El historial de impuestos no puede eliminarse");
        }

        public override List<T> RetrieveAll<T>()
        {
            var lsTaxes = new List<T>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_TAX_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in lsResults)
            {
                var tax = BuildTax(row);
                lsTaxes.Add((T)Convert.ChangeType(tax, typeof(T)));
            }

            return lsTaxes;
        }

        public override T RetrieveById<T>(int id)
        {
            throw new NotSupportedException("Use RetrieveAll o RetrieveActive para consultar impuestos");
        }

        // Impuesto actualmente vigente, o null si nunca se ha registrado ninguno
        public Tax RetrieveActive()
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ACTIVE_TAX_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lsResults.Count > 0)
            {
                return BuildTax(lsResults[0]);
            }

            return null;
        }

        private Tax BuildTax(Dictionary<string, object> row)
        {
            return new Tax
            {
                Id = (int)row["TaxId"],
                Name = (string)row["Name"],
                Percentage = Convert.ToDecimal(row["Percentage"]),
                ValidFrom = (DateTime)row["ValidFrom"],
                ValidTo = row["ValidTo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["ValidTo"]) : null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = (DateTime)row["CreatedAt"]
            };
        }
    }
}
