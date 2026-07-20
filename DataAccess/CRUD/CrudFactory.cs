using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.DAO;
using Entities_DTOs;

namespace DataAccess.CRUD
{
    // Padre o madre, de los cruds que vamos a tener en la arquitectura.
    // Define el contrato que debe implementar todo CRUD.
    public abstract class CrudFactory
    {
        protected SqlDao sqlDao;


        //Definir los métodos que forman parte del contrato.

        // Create
        // Retrieve o conculta
        // Update
        // Delete

        public abstract void Create(BaseDTO baseDTO);
        public abstract void Update(BaseDTO baseDTO);
        public abstract void Delete(BaseDTO baseDTO);
        public abstract T RetrieveById<T>(int id);
        public abstract List<T> RetrieveAll<T>();



    }
}
