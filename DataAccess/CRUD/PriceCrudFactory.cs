using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    // El historial de precios es de solo alta (no se edita ni se borra un precio ya
    // registrado, solo se cierra automáticamente al registrar uno nuevo - ver CRE_PRICE_PR).
    public class PriceCrudFactory : CrudFactory
    {
        public PriceCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var price = baseDTO as Price;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_PRICE_PR";

            sqlOperation.AddDecimalParameter("PriceCRCPerMWh", price.PriceCRCPerMWh);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Update(BaseDTO baseDTO)
        {
            throw new Exception("El historial de precios no puede modificarse");
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new Exception("El historial de precios no puede eliminarse");
        }

        public override List<T> RetrieveAll<T>()
        {
            var lsPrices = new List<T>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_PRICE_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in lsResults)
            {
                var price = BuildPrice(row);
                lsPrices.Add((T)Convert.ChangeType(price, typeof(T)));
            }

            return lsPrices;
        }

        public override T RetrieveById<T>(int id)
        {
            throw new NotSupportedException("Use RetrieveAll o RetrieveActive para consultar precios");
        }

        // Precio actualmente vigente, o null si nunca se ha registrado ninguno
        public Price RetrieveActive()
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ACTIVE_PRICE_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lsResults.Count > 0)
            {
                return BuildPrice(lsResults[0]);
            }

            return null;
        }

        private Price BuildPrice(Dictionary<string, object> row)
        {
            return new Price
            {
                Id = (int)row["PriceId"],
                PriceCRCPerMWh = Convert.ToDecimal(row["PriceCRCPerMWh"]),
                ValidFrom = (DateTime)row["ValidFrom"],
                ValidTo = row["ValidTo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["ValidTo"]) : null,
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedAt = (DateTime)row["CreatedAt"]
            };
        }
    }
}
