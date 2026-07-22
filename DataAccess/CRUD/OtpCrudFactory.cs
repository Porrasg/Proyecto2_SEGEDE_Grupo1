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
            sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            // Convirtiendo el baseDTO en un objeto OtpToken
            var otp = baseDTO as OtpToken;

            // Definir el SP por medio del sql operation
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "CRE_OTP_TOKEN_PR";

            // Mapeo exacto con los nombres del Stored Procedure en la BD
            sqlOperation.AddStringParameter("P_EMAIL", otp.Email);
            sqlOperation.AddStringParameter("P_TOKEN_CODE", otp.TokenCode);
            sqlOperation.AddStringParameter("P_PURPOSE", otp.Purpose);
            sqlOperation.AddDateTimeParameter("P_EXPIRATION_DATE", otp.ExpirationDate);

            // Ejecutamos el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Busca un OTP valido para el correo y codigo que se reciben
        // Si no lo encuentra, retorna null para que la capa superior maneje el error
        public OtpToken RetrieveValidOtp(string email, string tokenCode, string purpose)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();
            sqlOperation.ProcedureName = "RET_VALID_OTP_TOKEN_PR";
            sqlOperation.AddStringParameter("P_EMAIL", email);
            sqlOperation.AddStringParameter("P_TOKEN_CODE", tokenCode);
            sqlOperation.AddStringParameter("P_PURPOSE", purpose);

            // Ejecutar el SP
            var lsResults = sqlDao.ExecuteQueryProcedure(sqlOperation);

            // Si se obtiene un resultado, convertir la primera fila en un objeto OtpToken y devolverlo, de lo contrario devolver null
            if (lsResults.Count > 0)
            {
                var row = lsResults[0];
                return new OtpToken()
                {
                    Id = (int)row["Id"],
                    CreatedAt = (DateTime)row["Created"],
                    Email = (string)row["Email"],
                    TokenCode = (string)row["TokenCode"],
                    Purpose = (string)row["Purpose"],
                    ExpirationDate = (DateTime)row["ExpirationDate"],
                    IsUsed = (bool)row["IsUsed"]
                };
            }
            return null;
        }

        // Marca el OTP como utilizado
        public void MarkAsUsed(OtpToken otp)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_USED_OTP_TOKEN_PR";

            sqlOperation.AddIntParameter("P_OTP_ID", otp.Id);
            sqlOperation.AddNullableDateTimeParameter("P_USED_DATE", otp.UpdatedAt);

            // Ejecutar el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Invalida OTP anteriores del correo y propósito
        public void InvalidatePreviousOtps(string email, string purpose)
        {
            // Definir el SP
            var sqlOperation = new SqlOperation();

            sqlOperation.ProcedureName = "UPD_INVALIDATE_OTP_TOKEN_PR";

            sqlOperation.AddStringParameter("P_EMAIL", email);
            sqlOperation.AddStringParameter("P_PURPOSE", purpose);

            // Ejecutar el SP
            sqlDao.ExecuteProcedure(sqlOperation);
        }

        // Estos metodos no se usan para OTP, pero hay que implementarlos
        // porque la clase base los exige como parte del contrato del CRUD
        public override void Update(BaseDTO baseDTO) { throw new NotImplementedException(); }
        public override void Delete(BaseDTO baseDTO) { throw new NotImplementedException(); }
        public override T RetrieveById<T>(int id) { throw new NotImplementedException(); }
        public override List<T> RetrieveAll<T>() { throw new NotImplementedException(); }
    }
}
