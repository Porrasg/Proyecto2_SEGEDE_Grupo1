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


        public Audit RetrieveById(int id)
        {
            // Validar el identificador de la auditoría
            if (id <= 0)
            {
                throw new Exception(
                    "El identificador de la auditoría no es válido");
            }

            var aCrud = new AuditCrudFactory();

            // Obtener el registro de auditoría
            var audit = aCrud.RetrieveById<Audit>(id);

            // Validar que el registro exista
            if (audit == null)
            {
                throw new Exception(
                    "No se encontró el registro de auditoría solicitado");
            }

            return audit;
        }


        public void Create(Audit a)
        {
            // Validar que la auditoría no sea nula
            if (a == null)
            {
                throw new Exception(
                    "El registro de auditoría no puede ser nulo");
            }

            // Validar los campos obligatorios
            if (HasEmptyFields(a))
            {
                throw new Exception(
                    "Todos los campos obligatorios deben estar completos");
            }

            // Validar el identificador del usuario cuando se proporciona
            if (a.UserId.HasValue && a.UserId.Value <= 0)
            {
                throw new Exception(
                    "El identificador del usuario no es válido");
            }

            // Validar el identificador de la entidad cuando se proporciona
            if (a.EntityId.HasValue && a.EntityId.Value <= 0)
            {
                throw new Exception(
                    "El identificador de la entidad no es válido");
            }

            // Validar la relación de la entidad
            if (HasInvalidEntityReference(a))
            {
                throw new Exception(
                    "El nombre y el identificador de la entidad deben completarse correctamente");
            }

            // Asignar la fecha de creación
            a.CreatedAt = DateTime.Now;

            var aCrud = new AuditCrudFactory();

            // Crear el registro de auditoría
            aCrud.Create(a);
        }


        public void Update(Audit a)
        {
            // Los registros de auditoría no deben modificarse
            throw new Exception(
                "Los registros de auditoría no pueden ser modificados");
        }


        public void Delete(Audit a)
        {
            // Los registros de auditoría no deben eliminarse
            throw new Exception(
                "Los registros de auditoría no pueden ser eliminados");
        }


        // Validaciones
        private bool HasEmptyFields(Audit audit)
        {
            return string.IsNullOrWhiteSpace(audit.Action) ||
                   string.IsNullOrWhiteSpace(audit.EntityName) ||
                   string.IsNullOrWhiteSpace(audit.Description);
        }


        private bool HasInvalidEntityReference(Audit audit)
        {
            return audit.EntityId.HasValue &&
                   string.IsNullOrWhiteSpace(audit.EntityName);
        }
    }
}