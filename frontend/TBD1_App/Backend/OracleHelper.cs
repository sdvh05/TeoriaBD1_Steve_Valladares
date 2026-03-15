using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace TBD1_App.Backend
{
    public static class OracleHelper
    {
        private static readonly string ConnectionString =
            "Data Source=(DESCRIPTION=" +
                "(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))" +
                "(CONNECT_DATA=(SERVICE_NAME=XE)));" +
            "User Id=system;" +
            "Password=OracleTBD;";

        // ─────────────────────────────────────────
        // Abre y devuelve una conexión a Oracle
        // ─────────────────────────────────────────
        public static OracleConnection GetConnection()
        {
            var conn = new OracleConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        // ─────────────────────────────────────────
        // Ejecuta un SELECT y devuelve un DataTable
        // ─────────────────────────────────────────
        public static DataTable ExecuteQuery(string sql, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd  = new OracleCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                var dt      = new DataTable();
                var adapter = new OracleDataAdapter(cmd);
                adapter.Fill(dt);
                return dt;
            }
        }

        // ─────────────────────────────────────────
        // Ejecuta un SP sin valor de retorno
        // ─────────────────────────────────────────
        public static void ExecuteProcedure(string procedureName, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd  = new OracleCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                cmd.ExecuteNonQuery();
            }
        }

        // ─────────────────────────────────────────
        // Ejecuta un SP y retorna el parámetro OUT
        // Usado para INSERTs que retornan el ID generado
        // ─────────────────────────────────────────
        public static string ExecuteProcedureWithOutput(string procedureName, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd  = new OracleCommand(procedureName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                cmd.ExecuteNonQuery();

                // El parámetro OUT siempre se llama P_ID_GENERADO
                return cmd.Parameters["P_ID_GENERADO"].Value?.ToString();
            }
        }

        // ─────────────────────────────────────────
        // Ejecuta un SELECT y devuelve un solo valor
        // Útil para COUNT, SUM, funciones Oracle
        // ─────────────────────────────────────────
        public static object ExecuteScalar(string sql, params OracleParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd  = new OracleCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteScalar();
            }
        }

        // ─────────────────────────────────────────
        // Prueba si la conexión funciona
        // Llamar al iniciar la aplicación
        // ─────────────────────────────────────────
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                    return conn.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
