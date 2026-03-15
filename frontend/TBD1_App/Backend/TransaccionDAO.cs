using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Models;

namespace TBD1_App.Backend
{
    public class TransaccionDAO
    {
        // ─────────────────────────────────────────
        // Obtener transacciones de un presupuesto
        // filtradas por año y mes
        // ─────────────────────────────────────────
        public List<Transaccion> ObtenerPorMes(string idPresupuesto, int anio, int mes)
        {
            var lista = new List<Transaccion>();
            var dt    = OracleHelper.ExecuteQuery(@"
                SELECT  T.ID_TRANSACCION, T.ID_USER, T.ID_PRESUPUESTO,
                        T.ID_SUBCATEGORIA, T.ANIO, T.MES, T.TIPO,
                        T.DESCRIPCION, T.MONTO, T.FECHA_TRANSACCION,
                        T.METODO_PAGO, T.NUM_FACTURA, T.OBSERVACIONES,
                        T.FECHA_HORA_REGISTRO,
                        S.NOMBRE AS NOMBRE_SUBCATEGORIA,
                        C.NOMBRE AS NOMBRE_CATEGORIA
                FROM    TRANSACCION  T
                JOIN    SUBCATEGORIA S ON T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN    CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE   T.ID_PRESUPUESTO = :idP
                AND     T.ANIO           = :anio
                AND     T.MES            = :mes
                ORDER   BY T.FECHA_TRANSACCION DESC",
                new OracleParameter("idP",  idPresupuesto),
                new OracleParameter("anio", anio),
                new OracleParameter("mes",  mes));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Obtener transacciones filtradas por tipo
        // tipo: 'ingreso' / 'gasto' / 'ahorro'
        // ─────────────────────────────────────────
        public List<Transaccion> ObtenerPorTipo(string idPresupuesto, int anio, int mes, string tipo)
        {
            var lista = new List<Transaccion>();
            var dt    = OracleHelper.ExecuteQuery(@"
                SELECT  T.ID_TRANSACCION, T.ID_USER, T.ID_PRESUPUESTO,
                        T.ID_SUBCATEGORIA, T.ANIO, T.MES, T.TIPO,
                        T.DESCRIPCION, T.MONTO, T.FECHA_TRANSACCION,
                        T.METODO_PAGO, T.NUM_FACTURA, T.OBSERVACIONES,
                        T.FECHA_HORA_REGISTRO,
                        S.NOMBRE AS NOMBRE_SUBCATEGORIA,
                        C.NOMBRE AS NOMBRE_CATEGORIA
                FROM    TRANSACCION  T
                JOIN    SUBCATEGORIA S ON T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN    CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE   T.ID_PRESUPUESTO = :idP
                AND     T.ANIO           = :anio
                AND     T.MES            = :mes
                AND     T.TIPO           = :tipo
                ORDER   BY T.FECHA_TRANSACCION DESC",
                new OracleParameter("idP",  idPresupuesto),
                new OracleParameter("anio", anio),
                new OracleParameter("mes",  mes),
                new OracleParameter("tipo", tipo));

            foreach (DataRow row in dt.Rows)
                lista.Add(MapearFila(row));

            return lista;
        }

        // ─────────────────────────────────────────
        // Insertar transacción
        // Oracle genera el ID automaticamente (OUT)
        // ─────────────────────────────────────────
        public string Insertar(Transaccion t)
        {
            var pIdGenerado = new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
            {
                Direction = ParameterDirection.Output
            };

            OracleHelper.ExecuteProcedureWithOutput("SP_INSERTAR_TRANSACCION",
                new OracleParameter("P_ID_USER",         t.IdUser),
                new OracleParameter("P_ID_PRESUPUESTO",  t.IdPresupuesto),
                new OracleParameter("P_ID_SUBCATEGORIA", t.IdSubcategoria),
                new OracleParameter("P_ANIO",            t.Anio),
                new OracleParameter("P_MES",             t.Mes),
                new OracleParameter("P_TIPO",            t.Tipo),
                new OracleParameter("P_DESCRIPCION",     t.Descripcion),
                new OracleParameter("P_MONTO",           t.Monto),
                new OracleParameter("P_FECHA",           t.FechaTransaccion),
                new OracleParameter("P_METODO_PAGO",     t.MetodoPago),
                new OracleParameter("P_NUM_FACTURA",     (object)t.NumFactura    ?? DBNull.Value),
                new OracleParameter("P_OBSERVACIONES",   (object)t.Observaciones ?? DBNull.Value),
                pIdGenerado
            );

            return pIdGenerado.Value?.ToString();
        }

        // ─────────────────────────────────────────
        // Actualizar transacción
        // ─────────────────────────────────────────
        public void Actualizar(Transaccion t)
        {
            OracleHelper.ExecuteProcedure("SP_ACTUALIZAR_TRANSACCION",
                new OracleParameter("P_ID_TRANSACCION", t.IdTransaccion),
                new OracleParameter("P_DESCRIPCION",    t.Descripcion),
                new OracleParameter("P_MONTO",          t.Monto),
                new OracleParameter("P_METODO_PAGO",    t.MetodoPago),
                new OracleParameter("P_NUM_FACTURA",    (object)t.NumFactura    ?? DBNull.Value),
                new OracleParameter("P_OBSERVACIONES",  (object)t.Observaciones ?? DBNull.Value)
            );
        }

        // ─────────────────────────────────────────
        // Eliminar transacción
        // ─────────────────────────────────────────
        public void Eliminar(string idTransaccion)
        {
            OracleHelper.ExecuteProcedure("SP_ELIMINAR_TRANSACCION",
                new OracleParameter("P_ID_TRANSACCION", idTransaccion));
        }

        // ─────────────────────────────────────────
        // Mapeo DataRow → Transaccion
        // ─────────────────────────────────────────
        private Transaccion MapearFila(DataRow row)
        {
            return new Transaccion
            {
                IdTransaccion      = row["ID_TRANSACCION"].ToString(),
                IdUser             = row["ID_USER"].ToString(),
                IdPresupuesto      = row["ID_PRESUPUESTO"].ToString(),
                IdSubcategoria     = row["ID_SUBCATEGORIA"].ToString(),
                Anio               = Convert.ToInt32(row["ANIO"]),
                Mes                = Convert.ToInt32(row["MES"]),
                Tipo               = row["TIPO"].ToString(),
                Descripcion        = row["DESCRIPCION"].ToString(),
                Monto              = Convert.ToDecimal(row["MONTO"]),
                FechaTransaccion   = Convert.ToDateTime(row["FECHA_TRANSACCION"]),
                MetodoPago         = row["METODO_PAGO"].ToString(),
                NumFactura         = row["NUM_FACTURA"]    == DBNull.Value ? null : row["NUM_FACTURA"].ToString(),
                Observaciones      = row["OBSERVACIONES"]  == DBNull.Value ? null : row["OBSERVACIONES"].ToString(),
                FechaHoraRegistro  = Convert.ToDateTime(row["FECHA_HORA_REGISTRO"]),
                NombreSubcategoria = row["NOMBRE_SUBCATEGORIA"].ToString(),
                NombreCategoria    = row["NOMBRE_CATEGORIA"].ToString()
            };
        }
    }
}
