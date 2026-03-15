using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Models;

namespace TBD1_App.Backend
{
    public class UsuarioDAO
    {
        // ─────────────────────────────────────────
        // Obtener todos los usuarios activos
        // ─────────────────────────────────────────
        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            var dt    = OracleHelper.ExecuteQuery(
                "SELECT * FROM USUARIO WHERE ESTADO = 1 ORDER BY PRIMER_APELLIDO, PRIMER_NOMBRE");

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Obtener un usuario por ID
        // ─────────────────────────────────────────
        public Usuario ObtenerPorId(string idUser)
        {
            var dt = OracleHelper.ExecuteQuery(
                "SELECT * FROM USUARIO WHERE ID_USER = :id",
                new OracleParameter("id", idUser));

            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        // ─────────────────────────────────────────
        // Login — busca por correo y password
        // ─────────────────────────────────────────
        public Usuario Login(string correo, string password)
        {
            var dt = OracleHelper.ExecuteQuery(
                "SELECT * FROM USUARIO WHERE CORREO = :correo AND PASSWORD = :pass AND ESTADO = 1",
                new OracleParameter("correo", correo),
                new OracleParameter("pass",   password));

            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        // ─────────────────────────────────────────
        // Insertar nuevo usuario
        // Oracle genera el ID automaticamente (OUT)
        // ─────────────────────────────────────────
        public string Insertar(Usuario u)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_USUARIO",
                new OracleParameter("P_PRIMER_NOMBRE",    u.PrimerNombre),
                new OracleParameter("P_SEGUNDO_NOMBRE",   (object)u.SegundoNombre   ?? DBNull.Value),
                new OracleParameter("P_PRIMER_APELLIDO",  u.PrimerApellido),
                new OracleParameter("P_SEGUNDO_APELLIDO", (object)u.SegundoApellido ?? DBNull.Value),
                new OracleParameter("P_CORREO",           u.Correo),
                new OracleParameter("P_PASSWORD",         u.Password),
                new OracleParameter("P_SALARIO_BASE",     u.SalarioBase),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }

        // ─────────────────────────────────────────
        // Actualizar usuario existente
        // ─────────────────────────────────────────
        public void Actualizar(Usuario u)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_USUARIO",
                new OracleParameter("P_ID_USER",          u.IdUser),
                new OracleParameter("P_PRIMER_NOMBRE",    u.PrimerNombre),
                new OracleParameter("P_SEGUNDO_NOMBRE",   (object)u.SegundoNombre   ?? DBNull.Value),
                new OracleParameter("P_PRIMER_APELLIDO",  u.PrimerApellido),
                new OracleParameter("P_SEGUNDO_APELLIDO", (object)u.SegundoApellido ?? DBNull.Value),
                new OracleParameter("P_SALARIO_BASE",     u.SalarioBase)
            );
        }

        // ─────────────────────────────────────────
        // Desactivar usuario (soft delete)
        // ─────────────────────────────────────────
        public void Eliminar(string idUser)
        {
            OracleHelper.ExecuteProcedure("SP_ELIMINAR_USUARIO",
                new OracleParameter("P_ID_USER", idUser));
        }

        // ─────────────────────────────────────────
        // Mapeo DataRow → Usuario
        // ─────────────────────────────────────────
        private Usuario MapearFila(DataRow row)
        {
            return new Usuario
            {
                IdUser          = row["ID_USER"].ToString(),
                PrimerNombre    = row["PRIMER_NOMBRE"].ToString(),
                SegundoNombre   = row["SEGUNDO_NOMBRE"]  == DBNull.Value ? null : row["SEGUNDO_NOMBRE"].ToString(),
                PrimerApellido  = row["PRIMER_APELLIDO"].ToString(),
                SegundoApellido = row["SEGUNDO_APELLIDO"] == DBNull.Value ? null : row["SEGUNDO_APELLIDO"].ToString(),
                Correo          = row["CORREO"].ToString(),
                Password        = row["PASSWORD"].ToString(),
                FechaRegistro   = Convert.ToDateTime(row["FECHA_REGISTRO"]),
                SalarioBase     = Convert.ToDecimal(row["SALARIO_BASE"]),
                Estado          = Convert.ToInt32(row["ESTADO"])
            };
        }
    }
}
