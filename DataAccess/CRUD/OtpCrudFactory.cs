using DataAccess.DAO;
using Entities_DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.CRUD
{
    public class OtpCrudFactory : CrudFactory
    {
        public OtpCrudFactory()
        {
            // Uso del Singleton oficial del profesor
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var otp = baseDTO as OtpToken;
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_OTP_TOKEN_PR";

            // Enlace exacto de parámetros con prefijo P_
            sqlOperation.AddStringParameter("P_EMAIL", otp.Email);
            sqlOperation.AddStringParameter("P_TOKEN_CODE", otp.TokenCode);
            sqlOperation.AddDateTimeParameter("P_EXPIRATION_DATE", otp.ExpirationDate);

            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Método oficial para recuperar un OTP vigente y traducirlo a un objeto DTO limpio
        public OtpToken RetrieveValidOtp(string email, string tokenCode)
        {
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_VALID_OTP_PR";
            sqlOperation.AddStringParameter("P_EMAIL", email);
            sqlOperation.AddStringParameter("P_TOKEN_CODE", tokenCode);

            var lstResults = sqlDao.ExecuteQueryProcedure(sqlOperation);
            if (lstResults.Count > 0)
            {
                var row = lstResults[0]; // Extrae la primera fila del conjunto
                return new OtpToken()
                {
                    Id = (int)row["Id"],
                    CreatedAt = (DateTime)row["Created"],
                    Email = (string)row["Email"],
                    TokenCode = (string)row["TokenCode"],
                    ExpirationDate = (DateTime)row["ExpirationDate"],
                    IsUsed = (bool)row["IsUsed"]
                };
            }
            return null;
        }

        // Firmas abstractas obligatorias de la cátedra
        public override void Update(BaseDTO baseDTO) { throw new NotImplementedException(); }
        public override void Delete(BaseDTO baseDTO) { throw new NotImplementedException(); }
        public override T RetrieveById<T>(int id) { throw new NotImplementedException(); }
        public override List<T> RetrieveAll<T>() { throw new NotImplementedException(); }
    }
}
