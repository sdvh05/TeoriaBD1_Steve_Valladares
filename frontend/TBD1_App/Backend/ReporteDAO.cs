using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using TBD1_App.Models;

namespace TBD1_App.Backend
{
    public class ReporteDAO
    {
        // ── SP_REPORTE_1: Resumen Mensual Ingresos vs Gastos vs Ahorros ──
        public List<ReporteBalanceMes> ObtenerBalanceRango(
            string idUser, int anioIni, int mesIni, int anioFin, int mesFin)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT
                    T.ANIO,
                    T.MES,
                    SUM(CASE WHEN T.TIPO = 'ingreso' THEN T.MONTO ELSE 0 END) AS TOTAL_INGRESOS,
                    SUM(CASE WHEN T.TIPO = 'gasto'   THEN T.MONTO ELSE 0 END) AS TOTAL_GASTOS,
                    SUM(CASE WHEN T.TIPO = 'ahorro'  THEN T.MONTO ELSE 0 END) AS TOTAL_AHORROS,
                    SUM(CASE WHEN T.TIPO = 'ingreso' THEN T.MONTO ELSE 0 END) -
                    SUM(CASE WHEN T.TIPO = 'gasto'   THEN T.MONTO ELSE 0 END) -
                    SUM(CASE WHEN T.TIPO = 'ahorro'  THEN T.MONTO ELSE 0 END) AS BALANCE_FINAL
                FROM TRANSACCION T
                WHERE T.ID_USER = :idUser
                  AND (T.ANIO * 100 + T.MES) BETWEEN (:anioIni * 100 + :mesIni)
                                                  AND (:anioFin * 100 + :mesFin)
                GROUP BY T.ANIO, T.MES
                ORDER BY T.ANIO, T.MES",
                new OracleParameter("idUser",  idUser),
                new OracleParameter("anioIni", anioIni),
                new OracleParameter("mesIni",  mesIni),
                new OracleParameter("anioFin", anioFin),
                new OracleParameter("mesFin",  mesFin));

            var lista = new List<ReporteBalanceMes>();
            foreach (DataRow row in dt.Rows)
                lista.Add(new ReporteBalanceMes
                {
                    Anio          = Convert.ToInt32(row["ANIO"]),
                    Mes           = Convert.ToInt32(row["MES"]),
                    TotalIngresos = row["TOTAL_INGRESOS"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TOTAL_INGRESOS"]),
                    TotalGastos   = row["TOTAL_GASTOS"]   == DBNull.Value ? 0 : Convert.ToDecimal(row["TOTAL_GASTOS"]),
                    TotalAhorros  = row["TOTAL_AHORROS"]  == DBNull.Value ? 0 : Convert.ToDecimal(row["TOTAL_AHORROS"]),
                    BalanceFinal  = row["BALANCE_FINAL"]  == DBNull.Value ? 0 : Convert.ToDecimal(row["BALANCE_FINAL"])
                });
            return lista;
        }

        // ── SP_REPORTE_2: Distribución de Gastos por Categoría ──
        public List<ReporteGastoCategoria> ObtenerGastosCategoria(
            string idUser, int anio, int mes)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT
                    C.ID_CATEGORIA,
                    C.NOMBRE                             AS NOMBRE_CATEGORIA,
                    SUM(T.MONTO)                         AS TOTAL_GASTADO,
                    COUNT(T.ID_TRANSACCION)              AS NUM_TRANSACCIONES,
                    ROUND(SUM(T.MONTO) * 100.0 / NULLIF((
                        SELECT SUM(T2.MONTO)
                        FROM   TRANSACCION T2
                        WHERE  T2.ID_USER = :idUser2
                          AND  T2.ANIO = :anio2
                          AND  T2.MES  = :mes2
                          AND  T2.TIPO = 'gasto'
                    ), 0), 2)                            AS PORCENTAJE
                FROM TRANSACCION T
                JOIN SUBCATEGORIA S ON T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE T.ID_USER = :idUser
                  AND T.ANIO    = :anio
                  AND T.MES     = :mes
                  AND T.TIPO    = 'gasto'
                GROUP BY C.ID_CATEGORIA, C.NOMBRE
                ORDER BY TOTAL_GASTADO DESC",
                new OracleParameter("idUser2", idUser),
                new OracleParameter("anio2",   anio),
                new OracleParameter("mes2",    mes),
                new OracleParameter("idUser",  idUser),
                new OracleParameter("anio",    anio),
                new OracleParameter("mes",     mes));

            var lista = new List<ReporteGastoCategoria>();
            foreach (DataRow row in dt.Rows)
                lista.Add(new ReporteGastoCategoria
                {
                    IdCategoria      = row["ID_CATEGORIA"].ToString(),
                    NombreCategoria  = row["NOMBRE_CATEGORIA"].ToString(),
                    TotalGastado     = row["TOTAL_GASTADO"]     == DBNull.Value ? 0 : Convert.ToDecimal(row["TOTAL_GASTADO"]),
                    NumTransacciones = row["NUM_TRANSACCIONES"] == DBNull.Value ? 0 : Convert.ToInt32(row["NUM_TRANSACCIONES"]),
                    Porcentaje       = row["PORCENTAJE"]        == DBNull.Value ? 0 : Convert.ToDecimal(row["PORCENTAJE"])
                });
            return lista;
        }

        // ── SP_REPORTE_3: Análisis de Cumplimiento de Presupuesto ──
        public List<ReporteCumplimiento> ObtenerCumplimiento(
            string idUser, string idPresupuesto, int anio, int mes)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT
                    C.NOMBRE                                        AS NOMBRE_CATEGORIA,
                    C.TIPO                                          AS TIPO_CATEGORIA,
                    S.NOMBRE                                        AS NOMBRE_SUBCATEGORIA,
                    D.MONTO                                         AS MONTO_PRESUPUESTADO,
                    NVL((SELECT SUM(T.MONTO)
                         FROM   TRANSACCION T
                         WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                           AND  T.ID_PRESUPUESTO  = :idPre2
                           AND  T.ANIO = :anio2 AND T.MES = :mes2), 0) AS MONTO_EJECUTADO,
                    D.MONTO - NVL((SELECT SUM(T.MONTO)
                         FROM   TRANSACCION T
                         WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                           AND  T.ID_PRESUPUESTO  = :idPre3
                           AND  T.ANIO = :anio3 AND T.MES = :mes3), 0) AS DIFERENCIA,
                    ROUND(NVL((SELECT SUM(T.MONTO)
                         FROM   TRANSACCION T
                         WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                           AND  T.ID_PRESUPUESTO  = :idPre4
                           AND  T.ANIO = :anio4 AND T.MES = :mes4), 0)
                         * 100.0 / NULLIF(D.MONTO, 0), 2)           AS PORCENTAJE_EJECUCION,
                    D.JUSTIFICACION
                FROM PRESUPUESTO_DETALLE D
                JOIN SUBCATEGORIA S ON D.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE D.ID_PRESUPUESTO = :idPre
                ORDER BY C.TIPO, C.NOMBRE, S.NOMBRE",
                new OracleParameter("idPre2", idPresupuesto),
                new OracleParameter("anio2",  anio),
                new OracleParameter("mes2",   mes),
                new OracleParameter("idPre3", idPresupuesto),
                new OracleParameter("anio3",  anio),
                new OracleParameter("mes3",   mes),
                new OracleParameter("idPre4", idPresupuesto),
                new OracleParameter("anio4",  anio),
                new OracleParameter("mes4",   mes),
                new OracleParameter("idPre",  idPresupuesto));

            var lista = new List<ReporteCumplimiento>();
            foreach (DataRow row in dt.Rows)
                lista.Add(new ReporteCumplimiento
                {
                    NombreCategoria     = row["NOMBRE_CATEGORIA"].ToString(),
                    TipoCategoria       = row["TIPO_CATEGORIA"].ToString(),
                    NombreSubcategoria  = row["NOMBRE_SUBCATEGORIA"].ToString(),
                    MontoPresupuestado  = row["MONTO_PRESUPUESTADO"]  == DBNull.Value ? 0 : Convert.ToDecimal(row["MONTO_PRESUPUESTADO"]),
                    MontoEjecutado      = row["MONTO_EJECUTADO"]      == DBNull.Value ? 0 : Convert.ToDecimal(row["MONTO_EJECUTADO"]),
                    Diferencia          = row["DIFERENCIA"]           == DBNull.Value ? 0 : Convert.ToDecimal(row["DIFERENCIA"]),
                    PorcentajeEjecucion = row["PORCENTAJE_EJECUCION"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PORCENTAJE_EJECUCION"]),
                    Justificacion       = row["JUSTIFICACION"]        == DBNull.Value ? null : row["JUSTIFICACION"].ToString()
                });
            return lista;
        }

        // ── SP_REPORTE_4: Tendencia de Gastos por Categoría en el Tiempo ──
        public List<ReporteTendencia> ObtenerTendenciaGastos(
            string idUser, int anioIni, int mesIni, int anioFin, int mesFin)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT
                    T.ANIO,
                    T.MES,
                    C.ID_CATEGORIA,
                    C.NOMBRE     AS NOMBRE_CATEGORIA,
                    SUM(T.MONTO) AS TOTAL_GASTADO
                FROM TRANSACCION T
                JOIN SUBCATEGORIA S ON T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE T.ID_USER = :idUser
                  AND T.TIPO    = 'gasto'
                  AND (T.ANIO * 100 + T.MES) BETWEEN (:anioIni * 100 + :mesIni)
                                                  AND (:anioFin * 100 + :mesFin)
                GROUP BY T.ANIO, T.MES, C.ID_CATEGORIA, C.NOMBRE
                ORDER BY T.ANIO, T.MES, C.NOMBRE",
                new OracleParameter("idUser",  idUser),
                new OracleParameter("anioIni", anioIni),
                new OracleParameter("mesIni",  mesIni),
                new OracleParameter("anioFin", anioFin),
                new OracleParameter("mesFin",  mesFin));

            var lista = new List<ReporteTendencia>();
            foreach (DataRow row in dt.Rows)
                lista.Add(new ReporteTendencia
                {
                    Anio            = Convert.ToInt32(row["ANIO"]),
                    Mes             = Convert.ToInt32(row["MES"]),
                    IdCategoria     = row["ID_CATEGORIA"].ToString(),
                    NombreCategoria = row["NOMBRE_CATEGORIA"].ToString(),
                    TotalGastado    = row["TOTAL_GASTADO"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TOTAL_GASTADO"])
                });
            return lista;
        }

        // ── SP_REPORTE_5: Estado de Obligaciones Fijas y Cumplimiento ──
        public List<ReporteObligacion> ObtenerEstadoObligaciones(
            string idUser, int anio, int mes)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT
                    O.ID_OBLIGACION,
                    O.NOMBRE                                    AS NOMBRE_OBLIGACION,
                    C.NOMBRE                                    AS NOMBRE_CATEGORIA,
                    S.NOMBRE                                    AS NOMBRE_SUBCATEGORIA,
                    O.MONTO,
                    O.DIA                                       AS DIA_VENCIMIENTO,
                    FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION)  AS DIAS_RESTANTES,
                    CASE
                        WHEN EXISTS (
                            SELECT 1 FROM OBLIGACION_TRANSACCION OT
                            JOIN TRANSACCION T ON OT.ID_TRANSACCION = T.ID_TRANSACCION
                            WHERE OT.ID_OBLIGACION = O.ID_OBLIGACION
                              AND T.ANIO = :anio AND T.MES = :mes
                        ) THEN 'PAGADA'
                        WHEN FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION) < 0  THEN 'VENCIDA'
                        WHEN FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION) <= 3 THEN 'POR VENCER'
                        ELSE 'PENDIENTE'
                    END                                         AS ESTADO_PAGO,
                    (SELECT MAX(T2.FECHA_TRANSACCION)
                     FROM OBLIGACION_TRANSACCION OT2
                     JOIN TRANSACCION T2 ON OT2.ID_TRANSACCION = T2.ID_TRANSACCION
                     WHERE OT2.ID_OBLIGACION = O.ID_OBLIGACION) AS FECHA_ULTIMO_PAGO,
                    O.FECHA_INICIO,
                    O.FECHA_FIN
                FROM OBLIGACION_FIJA O
                JOIN SUBCATEGORIA    S ON O.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN CATEGORIA       C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                WHERE O.ID_USER = :idUser
                  AND O.VIGENTE = 1
                ORDER BY O.DIA",
                new OracleParameter("anio",   anio),
                new OracleParameter("mes",    mes),
                new OracleParameter("idUser", idUser));

            var lista = new List<ReporteObligacion>();
            foreach (DataRow row in dt.Rows)
                lista.Add(new ReporteObligacion
                {
                    IdObligacion       = row["ID_OBLIGACION"].ToString(),
                    NombreObligacion   = row["NOMBRE_OBLIGACION"].ToString(),
                    NombreCategoria    = row["NOMBRE_CATEGORIA"].ToString(),
                    NombreSubcategoria = row["NOMBRE_SUBCATEGORIA"].ToString(),
                    Monto              = row["MONTO"]            == DBNull.Value ? 0 : Convert.ToDecimal(row["MONTO"]),
                    DiaVencimiento     = row["DIA_VENCIMIENTO"]  == DBNull.Value ? 0 : Convert.ToInt32(row["DIA_VENCIMIENTO"]),
                    DiasRestantes      = row["DIAS_RESTANTES"]   == DBNull.Value ? 0 : Convert.ToInt32(row["DIAS_RESTANTES"]),
                    EstadoPago         = row["ESTADO_PAGO"]      == DBNull.Value ? "" : row["ESTADO_PAGO"].ToString(),
                    FechaUltimoPago    = row["FECHA_ULTIMO_PAGO"]== DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FECHA_ULTIMO_PAGO"]),
                    FechaInicio        = Convert.ToDateTime(row["FECHA_INICIO"]),
                    FechaFin           = row["FECHA_FIN"]        == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FECHA_FIN"])
                });
            return lista;
        }

        // ── SP_REPORTE_6: Progreso de Metas de Ahorro ──
        public List<ReporteProgresoAhorro> ObtenerProgresoAhorro(
            string idUser, string idPresupuesto)
        {
            var dt = OracleHelper.ExecuteQuery(@"
                SELECT
                    S.ID_SUBCATEGORIA,
                    S.NOMBRE                                        AS NOMBRE_META,
                    C.NOMBRE                                        AS NOMBRE_CATEGORIA,
                    D.MONTO                                         AS MONTO_OBJETIVO_MENSUAL,
                    ((P.ANIO_FIN - P.ANIO_INICIO) * 12 +
                     (P.MES_FIN  - P.MES_INICIO)  + 1)             AS MESES_VIGENCIA,
                    D.MONTO * ((P.ANIO_FIN - P.ANIO_INICIO) * 12 +
                               (P.MES_FIN  - P.MES_INICIO)  + 1)   AS MONTO_OBJETIVO_TOTAL,
                    NVL((SELECT SUM(T.MONTO)
                         FROM TRANSACCION T
                         WHERE T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                           AND T.ID_PRESUPUESTO  = :idPre2
                           AND T.TIPO = 'ahorro'
                           AND T.ID_USER = :idUser2), 0)            AS MONTO_ACUMULADO,
                    ROUND(NVL((SELECT SUM(T.MONTO)
                         FROM TRANSACCION T
                         WHERE T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                           AND T.ID_PRESUPUESTO  = :idPre3
                           AND T.TIPO = 'ahorro'
                           AND T.ID_USER = :idUser3), 0) * 100.0 /
                        NULLIF(D.MONTO * ((P.ANIO_FIN - P.ANIO_INICIO) * 12 +
                                          (P.MES_FIN  - P.MES_INICIO)  + 1), 0), 2) AS PORCENTAJE_COMPLETADO,
                    CAST((TO_DATE(P.ANIO_FIN || '-' || LPAD(P.MES_FIN,2,'0') || '-28','YYYY-MM-DD')
                     - SYSDATE) AS INTEGER)                         AS DIAS_RESTANTES,
                    P.ANIO_FIN,
                    P.MES_FIN
                FROM PRESUPUESTO_DETALLE D
                JOIN SUBCATEGORIA        S ON D.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                JOIN CATEGORIA           C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
                JOIN PRESUPUESTO         P ON D.ID_PRESUPUESTO  = P.ID_PRESUPUESTO
                WHERE D.ID_PRESUPUESTO = :idPre
                  AND C.TIPO = 'ahorro'
                ORDER BY PORCENTAJE_COMPLETADO DESC",
                new OracleParameter("idPre2",  idPresupuesto),
                new OracleParameter("idUser2", idUser),
                new OracleParameter("idPre3",  idPresupuesto),
                new OracleParameter("idUser3", idUser),
                new OracleParameter("idPre",   idPresupuesto));

            var lista = new List<ReporteProgresoAhorro>();
            foreach (DataRow row in dt.Rows)
                lista.Add(new ReporteProgresoAhorro
                {
                    IdSubcategoria       = row["ID_SUBCATEGORIA"].ToString(),
                    NombreMeta           = row["NOMBRE_META"].ToString(),
                    NombreCategoria      = row["NOMBRE_CATEGORIA"].ToString(),
                    MontoObjetivoMensual = row["MONTO_OBJETIVO_MENSUAL"] == DBNull.Value ? 0 : Convert.ToDecimal(row["MONTO_OBJETIVO_MENSUAL"]),
                    MesesVigencia        = row["MESES_VIGENCIA"]         == DBNull.Value ? 0 : Convert.ToInt32(row["MESES_VIGENCIA"]),
                    MontoObjetivoTotal   = row["MONTO_OBJETIVO_TOTAL"]   == DBNull.Value ? 0 : Convert.ToDecimal(row["MONTO_OBJETIVO_TOTAL"]),
                    MontoAcumulado       = row["MONTO_ACUMULADO"]        == DBNull.Value ? 0 : Convert.ToDecimal(row["MONTO_ACUMULADO"]),
                    PorcentajeCompletado = row["PORCENTAJE_COMPLETADO"]  == DBNull.Value ? 0 : Convert.ToDecimal(row["PORCENTAJE_COMPLETADO"]),
                    DiasRestantes        = row["DIAS_RESTANTES"]         == DBNull.Value ? 0 : (int)Math.Round(Convert.ToDouble(row["DIAS_RESTANTES"].ToString())),
                    AnioFin              = row["ANIO_FIN"]               == DBNull.Value ? DateTime.Now.Year  : Convert.ToInt32(row["ANIO_FIN"]),
                    MesFin               = row["MES_FIN"]                == DBNull.Value ? DateTime.Now.Month : Convert.ToInt32(row["MES_FIN"])
                });
            return lista;
        }
    }
}
