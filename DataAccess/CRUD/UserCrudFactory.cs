using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class UserCrudFactory : CrudFactory
    {

        public UserCrudFactory() {

            sqlDao = SqlDao.GetInstance();

        }
        public override void Create(BaseDTO baseDTO)
        {
            var user = baseDTO as User;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_USER_PR";

            sqlOperation.AddStringParameter("Identification", user.Identification);
            sqlOperation.AddStringParameter("FirstName", user.FirstName);
            sqlOperation.AddStringParameter("FirstLastName", user.FirstLastName);
            sqlOperation.AddStringParameter("SecondLastName", user.SecondLastName);
            sqlOperation.AddDateTimeParameter("BirthDate", user.BirthDate);
            sqlOperation.AddStringParameter("PhoneNumber", user.PhoneNumber);
            sqlOperation.AddStringParameter("Email", user.Email);
            sqlOperation.AddStringParameter("ProfilePhoto", user.ProfilePhoto);
            sqlOperation.AddStringParameter("PasswordHash", user.PasswordHash);
            sqlOperation.AddStringParameter("Role", user.Role);
            sqlOperation.AddStringParameter("Status", user.Status);
            sqlOperation.AddIntParameter("FailedLoginAttempts", user.FailedLoginAttempts);
            sqlOperation.AddDateTimeParameter("LockoutEndAt", user.LockoutEndAt ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("LastLoginAt", user.LastLoginAt ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("CreatedAt", user.CreatedAt);
            sqlOperation.AddDateTimeParameter("UpdatedAt", user.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            var user = baseDTO as User;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "DEL_USER_PR";

            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddStringParameter("Status", user.Status);
            sqlOperation.AddDateTimeParameter("UpdatedAt", user.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var list = new List<T>();
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_ALL_USER_PR";

            var results = sqlDao.ExecuteQueryProcedure(operation);
            if (results.Count > 0)
            {
                foreach (var row in results)
                {
                    var user = BuildUser(row);
                    list.Add((T)Convert.ChangeType(user, typeof(T)));
                }
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var operation = new SqlOperation();
            operation.ProcedureName = "RET_BY_ID_USER_PR";
            operation.AddIntParameter("UserId", id);
            var results = sqlDao.ExecuteQueryProcedure(operation);
            if (results.Count > 0)
            {
                var user = BuildUser(results[0]);
                return (T)Convert.ChangeType(user, typeof(T));
            }
            return default(T);
        }

        public override void Update(BaseDTO baseDTO)
        {
            var user = baseDTO as User;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "UPD_USER_PR";

            sqlOperation.AddIntParameter("UserId", user.Id);
            sqlOperation.AddStringParameter("Identification", user.Identification);
            sqlOperation.AddStringParameter("FirstName", user.FirstName);
            sqlOperation.AddStringParameter("FirstLastName", user.FirstLastName);
            sqlOperation.AddStringParameter("SecondLastName", user.SecondLastName);
            sqlOperation.AddDateTimeParameter("BirthDate", user.BirthDate);
            sqlOperation.AddStringParameter("PhoneNumber", user.PhoneNumber);
            sqlOperation.AddStringParameter("Email", user.Email);
            sqlOperation.AddStringParameter("ProfilePhoto", user.ProfilePhoto);
            sqlOperation.AddStringParameter("PasswordHash", user.PasswordHash);
            sqlOperation.AddStringParameter("Role", user.Role);
            sqlOperation.AddStringParameter("Status", user.Status);
            sqlOperation.AddIntParameter("FailedLoginAttempts", user.FailedLoginAttempts);
            sqlOperation.AddDateTimeParameter("LockoutEndAt", user.LockoutEndAt ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("LastLoginAt", user.LastLoginAt ?? default(DateTime));
            sqlOperation.AddDateTimeParameter("UpdatedAt", user.UpdatedAt);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        private User BuildUser(Dictionary<string, object> row)
        {
            var user = new User
            {
                Id = (int)row["UserId"],
                Identification = (string)row["Identification"],
                FirstName = (string)row["FirstName"],
                FirstLastName = (string)row["FirstLastName"],
                SecondLastName = (string)row["SecondLastName"],
                BirthDate = (DateTime)row["BirthDate"],
                PhoneNumber = (string)row["PhoneNumber"],
                Email = (string)row["Email"],
                ProfilePhoto = (string)row["ProfilePhoto"],
                PasswordHash = (string)row["PasswordHash"],
                Role = (string)row["Role"],
                Status = (string)row["Status"],
                FailedLoginAttempts = (int)row["FailedLoginAttempts"],
                LockoutEndAt = (DateTime)row["LockoutEndAt"],
                LastLoginAt = (DateTime)row["LastLoginAt"],
                CreatedAt = (DateTime)row["CreatedAt"],
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
            return user;
        }
    }
}
