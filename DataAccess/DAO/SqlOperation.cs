using System;
using System.Collections.Generic;
using System.Text;
using Entities_DTOs;
using Microsoft.Data.SqlClient;

namespace DataAccess.DAO
{
    // Clase que define el procesimiento a ejecutar y los parametros de este
    public class SqlOperation
    {
        public string ProcedureName { get; set; } = string.Empty;
        
        public List<SqlParameter> Parameters { get; set; } = new();

        public SqlOperation()
        {
            Parameters = new List<SqlParameter>();
        }

        public void AddStringParameter(string parameterName, string value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

        // Agrega un parámetro de tipo string que puede ser nulo
        public void AddNullableStringParameter(string parameterName, string? value)
        {
            Parameters.Add(new SqlParameter(parameterName, value ?? (object)DBNull.Value)
            );
        }

        public void AddIntParameter(string parameterName, int value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

        // Agrega un parámetro de tipo int que puede ser nulo
        public void AddNullableIntParameter(string parameterName, int? value)
        {
            Parameters.Add(new SqlParameter(parameterName, value.HasValue ? value.Value : DBNull.Value));
        }

        public void AddDoubleParameter(string parameterName, double value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

        public void AddDateTimeParameter(string parameterName, DateTime value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));

        }

        // Agrega un parámetro de tipo DateTime que puede ser nulo
        public void AddNullableDateTimeParameter(string parameterName, DateTime? value)
        {
            Parameters.Add( new SqlParameter(parameterName, value.HasValue ? value.Value : DBNull.Value)); 
        }

        public void AddDecimalParameter(string parameterName, decimal value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }
        // Parametros para que el flushconfig pueda usar TimeSpan y bool
        public void AddTimeSpanParameter(string parameterName, TimeSpan value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }
        public void AddBoolParameter(string parameterName, bool value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }
    }
}
