using DataAccess.CRUD;
using Entities_DTOs;
using System;
using System.Collections.Generic;

namespace CoreApp
{
    // Lógica de negocio de la matriz de Roles y Permisos (Admin/Roles)
    public class RoleManager
    {
        private static readonly string[] ValidRoles = { "Administrator", "Engineer", "Distributor" };

        public List<RolePermission> RetrieveAllPermissions()
        {
            var crud = new RolePermissionCrudFactory();
            return crud.RetrieveAll<RolePermission>();
        }

        public void SetPermission(string roleName, string moduleKey, string moduleLabel, bool canAccess)
        {
            if (Array.IndexOf(ValidRoles, roleName) < 0)
            {
                throw new Exception("El rol debe ser Administrator, Engineer o Distributor");
            }

            if (string.IsNullOrWhiteSpace(moduleKey))
            {
                throw new Exception("El módulo es requerido");
            }

            var crud = new RolePermissionCrudFactory();
            crud.Update(new RolePermission
            {
                RoleName = roleName,
                ModuleKey = moduleKey.Trim(),
                ModuleLabel = string.IsNullOrWhiteSpace(moduleLabel) ? moduleKey.Trim() : moduleLabel.Trim(),
                CanAccess = canAccess
            });
        }
    }
}
