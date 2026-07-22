using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class UserCrudFactory : CrudFactory
    {

        public UserCrudFactory()
        {

            sqlDao = SqlDao.GetInstance();

        }
        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto User
            var user = baseDTO as User;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_USER_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("Identification", user.Identification);
            sqlOperation.AddStringParameter("FirstName", user.FirstName);
            sqlOperation.AddStringParameter("FirstLastName", user.FirstLastName);
            sqlOperation.AddNullableStringParameter("SecondLastName", user.SecondLastName); // puede ser nulo
            sqlOperation.AddDateTimeParameter("BirthDate", user.BirthDate);
            sqlOperation.AddStringParameter("PhoneNumber", user.PhoneNumber);
            sqlOperation.AddStringParameter("Email", user.Email);
            sqlOperation.AddNullableStringParameter("ProfilePhoto", user.ProfilePhoto); // puede ser nulo
            sqlOperation.AddStringParameter("Password", user.Password);
            sqlOperation.AddIntParameter("Age", user.Age);
            sqlOperation.AddStringParameter("Role", user.Role);
            sqlOperation.AddStringParameter("Status", user.Status);
            sqlOperation.AddIntParameter("FailedLoginAttempts", user.FailedLoginAttempts);
            sqlOperation.AddNullableDateTimeParameter("LockoutEndAt", user.LockoutEndAt); // puede ser nulo
            sqlOperation.AddNullableDateTimeParameter("LastLoginAt", user.LastLoginAt); // puede ser nulo
            sqlOperation.AddDateTimeParameter("CreatedAt", user.CreatedAt);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", user.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            // Convertir el baseDTO en un objeto User
            var user = baseDTO as User;

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_USER_PR";

            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddStringParameter("Status", user.Status);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", user.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            // Lista que va a contener a todos los usuarios que se obtengan de la consulta a la BD
            var lsUsers = new List<T>();

            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_ALL_USER_PR";

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Recorrer la lista de resultados y convertir cada fila en un objeto User, luego agregarlo a la lista de usuarios
            if (lsResults.Count > 0)
            {
                foreach (var row in lsResults)
                {
                    var user = BuildUser(row);
                    lsUsers.Add((T)Convert.ChangeType(user, typeof(T)));
                }
            }

            return lsUsers;
        }

        public override T RetrieveById<T>(int id)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_ID_USER_PR";

            sqlOperation.AddIntParameter("UserId", id);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto User y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var user = BuildUser(lsResults[0]);
                return (T)Convert.ChangeType(user, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto User
            var user = baseDTO as User;

            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_USER_PR";

            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddStringParameter("Identification", user.Identification);
            sqlOperation.AddStringParameter("FirstName", user.FirstName);
            sqlOperation.AddStringParameter("FirstLastName", user.FirstLastName);
            sqlOperation.AddNullableStringParameter("SecondLastName", user.SecondLastName); // puede ser nulo
            sqlOperation.AddDateTimeParameter("BirthDate", user.BirthDate);
            sqlOperation.AddStringParameter("PhoneNumber", user.PhoneNumber);
            sqlOperation.AddStringParameter("Email", user.Email);
            sqlOperation.AddNullableStringParameter("ProfilePhoto", user.ProfilePhoto); // puede ser nulo
            sqlOperation.AddStringParameter("Password", user.Password);
            sqlOperation.AddIntParameter("Age", user.Age);
            sqlOperation.AddStringParameter("Role", user.Role);
            sqlOperation.AddStringParameter("Status", user.Status);
            sqlOperation.AddIntParameter("FailedLoginAttempts", user.FailedLoginAttempts);
            sqlOperation.AddNullableDateTimeParameter("LockoutEndAt", user.LockoutEndAt); // puede ser nulo
            sqlOperation.AddNullableDateTimeParameter("LastLoginAt", user.LastLoginAt); // puede ser nulo
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", user.UpdatedAt); // puede ser nulo

            //Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Método para recuperar un usuario por medio del correo electrónico
        public User? RetrieveByEmail(string email)
        {
            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_EMAIL_USER_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("Email", email);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto User
            // y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var user = BuildUser(lsResults[0]);
                return user;
            }

            return null;
        }

        // Método para recuperar un usuario por medio de la identificación
        public User RetrieveByIdentification(string identification)
        {
            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_BY_IDENTIFICATION_USER_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("Identification", identification);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto User
            // y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var user = BuildUser(lsResults[0]);
                return user;
            }

            return null;
        }

        // Método para actualizar los intentos fallidos de inicio de sesión
        public void UpdateLoginAttempts(User user)
        {
            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_LOGIN_ATTEMPTS_USER_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddIntParameter("FailedLoginAttempts", user.FailedLoginAttempts);
            sqlOperation.AddNullableDateTimeParameter("LockoutEndAt", user.LockoutEndAt); // puede ser nulo
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", user.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Método para actualizar la fecha del último inicio de sesión
        public void UpdateLastLogin(User user)
        {
            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_LAST_LOGIN_USER_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddNullableDateTimeParameter("LastLoginAt", user.LastLoginAt); // puede ser nulo
            sqlOperation.AddIntParameter("FailedLoginAttempts", user.FailedLoginAttempts);
            sqlOperation.AddNullableDateTimeParameter("LockoutEndAt", user.LockoutEndAt); // puede ser nulo
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", user.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Método para actualizar la contraseña del usuario
        public void UpdatePassword(User user)
        {
            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_PASSWORD_USER_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddStringParameter("Password", user.Password);
            sqlOperation.AddNullableDateTimeParameter("UpdatedAt", user.UpdatedAt); // puede ser nulo

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Activa la cuenta del usuario y valida el OTP en la base de datos.
        public void ActivateAccount(string email, string tokenCode)
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "ACTIVATE_USER_ACCOUNT_PR";

            sqlOperation.AddStringParameter("Email", email);
            sqlOperation.AddStringParameter("TokenCode", tokenCode);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        //Metodo que construye el DTO del User a partir de la data que viene en la consulta de la BD
        private User BuildUser(Dictionary<string, object> row)
        {
            var user = new User
            {
                Id = (int)row["UserId"],
                Identification = (string)row["Identification"],
                FirstName = (string)row["FirstName"],
                FirstLastName = (string)row["FirstLastName"],
                SecondLastName = row["SecondLastName"] != DBNull.Value ? (string)row["SecondLastName"] : null, // para manejar el caso de que SecondLastName pueda ser nulo
                BirthDate = (DateTime)row["BirthDate"],
                PhoneNumber = (string)row["PhoneNumber"],
                Email = (string)row["Email"],
                ProfilePhoto = row["ProfilePhoto"] != DBNull.Value ? (string)row["ProfilePhoto"] : null, // para manejar el caso de que ProfilePhoto pueda ser nulo
                Password = (string)row["Password"],
                Age = (int)row["Age"],
                Role = (string)row["Role"],
                Status = (string)row["Status"],
                FailedLoginAttempts = (int)row["FailedLoginAttempts"],
                LockoutEndAt = row["LockoutEndAt"] != DBNull.Value ? (DateTime?)row["LockoutEndAt"] : null, // para manejar el caso de que LockoutEndAt pueda ser nulo
                LastLoginAt = row["LastLoginAt"] != DBNull.Value ? (DateTime?)row["LastLoginAt"] : null, // para manejar el caso de que LastLoginAt pueda ser nulo
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = row["UpdatedAt"] != DBNull.Value ? (DateTime?)row["UpdatedAt"] : null // para manejar el caso de que UpdatedAt pueda ser nulo
            };
            return user;
        }
    }
}
