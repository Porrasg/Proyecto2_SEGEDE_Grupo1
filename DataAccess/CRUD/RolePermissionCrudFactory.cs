using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    // Matriz de permisos por rol y módulo. No sigue el patrón CRUD clásico (no hay
    // un registro individual que se "elimine"; se actualiza in-place vía upsert).
    public class RolePermissionCrudFactory : CrudFactory
    {
        public RolePermissionCrudFactory()
        {
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            Set(baseDTO as RolePermission);
        }

        public override void Update(BaseDTO baseDTO)
        {
            Set(baseDTO as RolePermission);
        }

        private void Set(RolePermission rp)
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_ROLE_PERMISSION_PR";

            sqlOperation.AddStringParameter("RoleName", rp.RoleName);
            sqlOperation.AddStringParameter("ModuleKey", rp.ModuleKey);
            sqlOperation.AddStringParameter("ModuleLabel", rp.ModuleLabel);
            sqlOperation.AddIntParameter("CanAccess", rp.CanAccess ? 1 : 0);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new NotSupportedException("Los permisos se desactivan (CanAccess = false), no se eliminan");
        }

        public override List<T> RetrieveAll<T>()
        {
            var lsPermissions = new List<T>();

            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_ROLE_PERMISSION_PR";

            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in lsResults)
            {
                var rp = BuildRolePermission(row);
                lsPermissions.Add((T)Convert.ChangeType(rp, typeof(T)));
            }

            return lsPermissions;
        }

        public override T RetrieveById<T>(int id)
        {
            throw new NotSupportedException("Use RetrieveAll para consultar la matriz de permisos");
        }

        private RolePermission BuildRolePermission(Dictionary<string, object> row)
        {
            return new RolePermission
            {
                Id = (int)row["RolePermissionId"],
                RoleName = (string)row["RoleName"],
                ModuleKey = (string)row["ModuleKey"],
                ModuleLabel = (string)row["ModuleLabel"],
                CanAccess = Convert.ToBoolean(row["CanAccess"]),
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["UpdatedAt"]) : null
            };
        }
    }
}
