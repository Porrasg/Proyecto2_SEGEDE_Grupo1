using System;
using System.Collections.Generic;
using System.Text;

using Entities_DTOs;
using DataAccess.CRUD;


namespace CoreApp
{
    public class AuditManager
    {
        public List<Audit> RetrieveAllAudits()
        {
            var aCrud = new AuditCrudFactory();

            return aCrud.RetrieveAll<Audit>();
        }

        public void Create(Audit a)
        {
            if (HasEmptyFields(a))
            {
                throw new Exception("Todos los campos obligatorios deben estar completos");
            }

            var aCrud = new AuditCrudFactory();

            aCrud.Create(a);
        }

        public void Update(Audit a)
        {
            var aCrud = new AuditCrudFactory();

            aCrud.Update(a);
        }
        public void Delete(Audit a)
        {
            var aCrud = new AuditCrudFactory();

            aCrud.Delete(a);
        }
        // Validaciones
        private bool HasEmptyFields(Audit audit)
        {
            return string.IsNullOrWhiteSpace(audit.Action) ||
                   string.IsNullOrWhiteSpace(audit.EntityName) ||
                   string.IsNullOrWhiteSpace(audit.Description);
        }
    }
}