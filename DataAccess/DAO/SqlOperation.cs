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
        public string ProcedureName { get; set; }
        public List<SqlParameter> Parameters { get; set; }

        public SqlOperation()
        {
            Parameters = new List<SqlParameter>();
        }

        // Solo vamos a trabjar con estos 4 tipos
        public void AddStringParameter(string parameterName, string value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

        public void AddIntParameter(string parameterName, int value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

        public void AddDoubleParameter(string parameterName, double value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

        public void AddDateTimeParameter(string parameterName, DateTime value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));

        }
        public void AddDecimalParameter(string parameterName, decimal value)
        {
            Parameters.Add(new SqlParameter(parameterName, value));
        }

       
    }
}
