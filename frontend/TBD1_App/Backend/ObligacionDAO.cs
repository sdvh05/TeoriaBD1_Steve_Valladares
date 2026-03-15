using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Models;

namespace TBD1_App.Backend
{
    public class ObligacionDAO
    {
        // ─────────────────────────────────────────
        // Obtener obligaciones vigentes de un usuario
        // ─────────────────────────────────────────
        public List<ObligacionFija> ObtenerActivas(string idUser)
        {
            var lista = new List<ObligacionFija>();
            var dt    = OracleHelper.ExecuteQuery(@"
                SELECT  O.ID_OBLIGACION, O.ID_USER, O.ID_SUBCATEGORIA,
                        O.NOMBRE, O.DESCRIPCION, O.MONTO,
                        O.DIA, O.VIGENTE, O.FECHA_INICIO, O.FECHA_FIN,
                        S.NOMBRE AS NOMBRE_SUBCATEGORIA,
                        C.NOMBRE AS NOMBRE_CATEGORIA,
                        FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION) AS DIAS_VENCIMIENTO
                FROM    OBLIGACION_FIJA O
                JOIN    SUBCATEGORIA    S ON O.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN    CATEGORIA       C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE   O.ID_USER = :id
                AND     O.VIGENTE = 1
                ORDER   BY O.DIA",
                new OracleParameter("id", idUser));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Obtener una obligación por ID
        // ─────────────────────────────────────────
        public ObligacionFija ObtenerPorId(string idObligacion)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT  O.ID_OBLIGACION, O.ID_USER, O.ID_SUBCATEGORIA,
                        O.NOMBRE, O.DESCRIPCION, O.MONTO,
                        O.DIA, O.VIGENTE, O.FECHA_INICIO, O.FECHA_FIN,
                        S.NOMBRE AS NOMBRE_SUBCATEGORIA,
                        C.NOMBRE AS NOMBRE_CATEGORIA,
                        FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION) AS DIAS_VENCIMIENTO
                FROM    OBLIGACION_FIJA O
                JOIN    SUBCATEGORIA    S ON O.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN    CATEGORIA       C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE   O.ID_OBLIGACION = :id",
                new OracleParameter("id", idObligacion));

            if (dt.Rows.Count == 0) return null;
            return MapearFila(dt.Rows[0]);
        }

        // ─────────────────────────────────────────
        // Insertar obligación fija
        // Oracle genera el ID automaticamente (OUT)
        // ─────────────────────────────────────────
        public string Insertar(ObligacionFija o)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_OBLIGACION",
                new OracleParameter("P_ID_USER",         o.IdUser),
                new OracleParameter("P_ID_SUBCATEGORIA", o.IdSubcategoria),
                new OracleParameter("P_NOMBRE",          o.Nombre),
                new OracleParameter("P_DESCRIPCION",     (object)o.Descripcion ?? DBNull.Value),
                new OracleParameter("P_MONTO",           o.Monto),
                new OracleParameter("P_DIA",             o.Dia),
                new OracleParameter("P_FECHA_INICIO",    o.FechaInicio),
                new OracleParameter("P_FECHA_FIN",       o.FechaFin.HasValue ? (object)o.FechaFin.Value : DBNull.Value),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }

        // ─────────────────────────────────────────
        // Actualizar obligación fija
        // ─────────────────────────────────────────
        public void Actualizar(ObligacionFija o)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_OBLIGACION",
                new OracleParameter("P_ID_OBLIGACION", o.IdObligacion),
                new OracleParameter("P_NOMBRE",        o.Nombre),
                new OracleParameter("P_DESCRIPCION",   (object)o.Descripcion ?? DBNull.Value),
                new OracleParameter("P_MONTO",         o.Monto),
                new OracleParameter("P_DIA",           o.Dia)
            );
        }

        // ─────────────────────────────────────────
        // Desactivar obligación (soft delete)
        // ─────────────────────────────────────────
        public void Desactivar(string idObligacion)
        {
            OracleHelper.ExecuteProcedure("SP_ELIMINAR_OBLIGACION",
                new OracleParameter("P_ID_OBLIGACION", idObligacion));
        }

        // ─────────────────────────────────────────
        // Mapeo DataRow → ObligacionFija
        // ─────────────────────────────────────────
        private ObligacionFija MapearFila(DataRow row)
        {
            return new ObligacionFija
            {
                IdObligacion       = row["ID_OBLIGACION"].ToString(),
                IdUser             = row["ID_USER"].ToString(),
                IdSubcategoria     = row["ID_SUBCATEGORIA"].ToString(),
                Nombre             = row["NOMBRE"].ToString(),
                Descripcion        = row["DESCRIPCION"]  == DBNull.Value ? null : row["DESCRIPCION"].ToString(),
                Monto              = Convert.ToDecimal(row["MONTO"]),
                Dia                = Convert.ToInt32(row["DIA"]),
                Vigente            = Convert.ToInt32(row["VIGENTE"]),
                FechaInicio        = Convert.ToDateTime(row["FECHA_INICIO"]),
                FechaFin           = row["FECHA_FIN"]    == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FECHA_FIN"]),
                NombreSubcategoria = row["NOMBRE_SUBCATEGORIA"].ToString(),
                NombreCategoria    = row["NOMBRE_CATEGORIA"].ToString(),
                DiasVencimiento    = row["DIAS_VENCIMIENTO"] == DBNull.Value ? 0 : Convert.ToInt32(row["DIAS_VENCIMIENTO"])
            };
        }
    }
}
