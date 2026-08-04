using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    // Historial de producción neta por turbina. Solo alta e histórico (no se edita ni
    // se borra un corte de producción ya calculado).
    public class EnergyProductionCrudFactory : CrudFactory
    {
        public EnergyProductionCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var ep = baseDTO as EnergyProduction;

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_ENERGY_PRODUCTION_PR";

            sqlOperation.AddIntParameter("TurbineId", ep.TurbineId);
            sqlOperation.AddDateTimeParameter("PeriodStart", ep.PeriodStart);
            sqlOperation.AddDateTimeParameter("EventDate", ep.EventDate);
            sqlOperation.AddDecimalParameter("GrossEnergyMWh", ep.GrossEnergyMWh);
            sqlOperation.AddDecimalParameter("MaintenanceLossMWh", ep.MaintenanceLossMWh);
            sqlOperation.AddDecimalParameter("GeneratedEnergy", ep.GeneratedEnergy);
            sqlOperation.AddDateTimeParameter("CreatedAt", ep.CreatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Update(BaseDTO baseDTO)
        {
            throw new Exception("El historial de producción no puede modificarse");
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new Exception("El historial de producción no puede eliminarse");
        }

        public override List<T> RetrieveAll<T>()
        {
            throw new NotSupportedException("Use RetrieveByTurbineId para consultar el historial de producción");
        }

        public override T RetrieveById<T>(int id)
        {
            throw new NotSupportedException("Use RetrieveByTurbineId para consultar el historial de producción");
        }

        public List<EnergyProduction> RetrieveByTurbineId(int turbineId)
        {
            var lsResults = new List<EnergyProduction>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_TURBINE_ENERGY_PRODUCTION_PR";
            sqlOperation.AddIntParameter("TurbineId", turbineId);

            var rows = sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in rows)
            {
                lsResults.Add(BuildEnergyProduction(row));
            }

            return lsResults;
        }

        // Último corte registrado para la turbina, o null si nunca se ha calculado producción
        public EnergyProduction RetrieveLast(int turbineId)
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_LAST_ENERGY_PRODUCTION_PR";
            sqlOperation.AddIntParameter("TurbineId", turbineId);

            var rows = sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (rows.Count > 0)
            {
                return BuildEnergyProduction(rows[0]);
            }

            return null;
        }

        private EnergyProduction BuildEnergyProduction(Dictionary<string, object> row)
        {
            return new EnergyProduction
            {
                Id = (int)row["EnergyProductionId"],
                TurbineId = Convert.ToInt32(row["TurbineId"]),
                PeriodStart = (DateTime)row["PeriodStart"],
                EventDate = (DateTime)row["EventDate"],
                GrossEnergyMWh = Convert.ToDecimal(row["GrossEnergyMWh"]),
                MaintenanceLossMWh = Convert.ToDecimal(row["MaintenanceLossMWh"]),
                GeneratedEnergy = Convert.ToDecimal(row["GeneratedEnergy"]),
                CreatedAt = (DateTime)row["CreatedAt"]
            };
        }
    }
}
