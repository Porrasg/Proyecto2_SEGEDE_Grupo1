using System;
using System.Collections.Generic;
using System.Text;

using Entities_DTOs;
using 


namespace CoreApp
{
    public class AuditManager
    {
        public List<Audit> RetrieveAllAudits()
        {
            var crud = new AuditCrudFactory();
            return crud.RetrieveAll<Audit>();
        }

        public Audit RetrieveById(int id)
        {
            var crud = new AuditCrudFactory();
            return crud.RetrieveById<Audit>(id);
        }

        public void Create(Audit audit)
        {
            // validaciones de negocio aquí

            var crud = new AuditCrudFactory();
            crud.Create(audit);
        }

        public void Update(Audit audit)
        {
            var crud = new AuditCrudFactory();
            crud.Update(audit);
        }

        public void Delete(Audit audit)
        {
            var crud = new AuditCrudFactory();
            crud.Delete(audit);
        }
    }
}