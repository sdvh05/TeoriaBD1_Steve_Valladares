-- ============================================================
-- REPORTE 1: Resumen Mensual Ingresos vs Gastos vs Ahorros
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_REPORTE_1 (
    P_ID_USER   IN  USUARIO.ID_USER%TYPE,
    P_ANIO_INI  IN  NUMBER,
    P_MES_INI   IN  NUMBER,
    P_ANIO_FIN  IN  NUMBER,
    P_MES_FIN   IN  NUMBER,
    P_CURSOR    OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN P_CURSOR FOR
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
        WHERE T.ID_USER = P_ID_USER
          AND (T.ANIO * 100 + T.MES) BETWEEN (P_ANIO_INI * 100 + P_MES_INI)
                                          AND (P_ANIO_FIN * 100 + P_MES_FIN)
        GROUP BY T.ANIO, T.MES
        ORDER BY T.ANIO, T.MES;
END SP_REPORTE_1;
/


-- ============================================================
-- REPORTE 2: Distribución de Gastos por Categoría
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_REPORTE_2 (
    P_ID_USER   IN  USUARIO.ID_USER%TYPE,
    P_ANIO      IN  NUMBER,
    P_MES       IN  NUMBER,
    P_CURSOR    OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            C.ID_CATEGORIA,
            C.NOMBRE                            AS NOMBRE_CATEGORIA,
            SUM(T.MONTO)                        AS TOTAL_GASTADO,
            COUNT(T.ID_TRANSACCION)             AS NUM_TRANSACCIONES,
            ROUND(
                SUM(T.MONTO) * 100.0 /
                NULLIF((
                    SELECT SUM(T2.MONTO)
                    FROM   TRANSACCION T2
                    WHERE  T2.ID_USER = P_ID_USER
                      AND  T2.ANIO    = P_ANIO
                      AND  T2.MES     = P_MES
                      AND  T2.TIPO    = 'gasto'
                ), 0), 2)                       AS PORCENTAJE
        FROM TRANSACCION T
        INNER JOIN SUBCATEGORIA S ON T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
        INNER JOIN CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
        WHERE T.ID_USER = P_ID_USER
          AND T.ANIO    = P_ANIO
          AND T.MES     = P_MES
          AND T.TIPO    = 'gasto'
        GROUP BY C.ID_CATEGORIA, C.NOMBRE
        ORDER BY TOTAL_GASTADO DESC;
END SP_REPORTE_2;
/


-- ============================================================
-- REPORTE 3: Cumplimiento de Presupuesto por Subcategoría
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_REPORTE_3 (
    P_ID_USER       IN  USUARIO.ID_USER%TYPE,
    P_ID_PRESUPUESTO IN PRESUPUESTO.ID_PRESUPUESTO%TYPE,
    P_ANIO          IN  NUMBER,
    P_MES           IN  NUMBER,
    P_CURSOR        OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            C.NOMBRE                                        AS NOMBRE_CATEGORIA,
            C.TIPO                                          AS TIPO_CATEGORIA,
            S.NOMBRE                                        AS NOMBRE_SUBCATEGORIA,
            D.MONTO                                         AS MONTO_PRESUPUESTADO,
            NVL((
                SELECT SUM(T.MONTO)
                FROM   TRANSACCION T
                WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                  AND  T.ID_PRESUPUESTO  = P_ID_PRESUPUESTO
                  AND  T.ANIO            = P_ANIO
                  AND  T.MES             = P_MES
            ), 0)                                           AS MONTO_EJECUTADO,
            D.MONTO - NVL((
                SELECT SUM(T.MONTO)
                FROM   TRANSACCION T
                WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                  AND  T.ID_PRESUPUESTO  = P_ID_PRESUPUESTO
                  AND  T.ANIO            = P_ANIO
                  AND  T.MES             = P_MES
            ), 0)                                           AS DIFERENCIA,
            ROUND(NVL((
                SELECT SUM(T.MONTO)
                FROM   TRANSACCION T
                WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                  AND  T.ID_PRESUPUESTO  = P_ID_PRESUPUESTO
                  AND  T.ANIO            = P_ANIO
                  AND  T.MES             = P_MES
            ), 0) * 100.0 / NULLIF(D.MONTO, 0), 2)        AS PORCENTAJE_EJECUCION,
            D.JUSTIFICACION
        FROM PRESUPUESTO_DETALLE D
        INNER JOIN SUBCATEGORIA S ON D.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
        INNER JOIN CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
        WHERE D.ID_PRESUPUESTO = P_ID_PRESUPUESTO
        ORDER BY C.TIPO, C.NOMBRE, S.NOMBRE;
END SP_REPORTE_3;
/


-- ============================================================
-- REPORTE 4: Tendencia de Gastos por Categoría en el Tiempo
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_REPORTE_4 (
    P_ID_USER   IN  USUARIO.ID_USER%TYPE,
    P_ANIO_INI  IN  NUMBER,
    P_MES_INI   IN  NUMBER,
    P_ANIO_FIN  IN  NUMBER,
    P_MES_FIN   IN  NUMBER,
    P_CURSOR    OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            T.ANIO,
            T.MES,
            C.ID_CATEGORIA,
            C.NOMBRE    AS NOMBRE_CATEGORIA,
            SUM(T.MONTO) AS TOTAL_GASTADO
        FROM TRANSACCION T
        INNER JOIN SUBCATEGORIA S ON T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
        INNER JOIN CATEGORIA    C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
        WHERE T.ID_USER = P_ID_USER
          AND T.TIPO    = 'gasto'
          AND (T.ANIO * 100 + T.MES) BETWEEN (P_ANIO_INI * 100 + P_MES_INI)
                                          AND (P_ANIO_FIN * 100 + P_MES_FIN)
        GROUP BY T.ANIO, T.MES, C.ID_CATEGORIA, C.NOMBRE
        ORDER BY T.ANIO, T.MES, C.NOMBRE;
END SP_REPORTE_4;
/


-- ============================================================
-- REPORTE 5: Estado de Obligaciones Fijas
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_REPORTE_5 (
    P_ID_USER   IN  USUARIO.ID_USER%TYPE,
    P_ANIO      IN  NUMBER,
    P_MES       IN  NUMBER,
    P_CURSOR    OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            O.ID_OBLIGACION,
            O.NOMBRE                                        AS NOMBRE_OBLIGACION,
            C.NOMBRE                                        AS NOMBRE_CATEGORIA,
            S.NOMBRE                                        AS NOMBRE_SUBCATEGORIA,
            O.MONTO,
            O.DIA                                           AS DIA_VENCIMIENTO,
            FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION)      AS DIAS_RESTANTES,
            -- Estado: pagada si existe transacción ese mes vinculada a la obligación
            CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM   OBLIGACION_TRANSACCION OT
                    INNER JOIN   TRANSACCION T ON OT.ID_TRANSACCION = T.ID_TRANSACCION
                    WHERE  OT.ID_OBLIGACION = O.ID_OBLIGACION
                      AND  T.ANIO = P_ANIO
                      AND  T.MES  = P_MES
                ) THEN 'PAGADA'
                WHEN FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION) < 0 THEN 'VENCIDA'
                WHEN FN_DIAS_HASTA_VENCIMIENTO(O.ID_OBLIGACION) <= 3 THEN 'POR VENCER'
                ELSE 'PENDIENTE'
            END                                             AS ESTADO_PAGO,
            -- Fecha del último pago registrado
            (
                SELECT MAX(T2.FECHA_TRANSACCION)
                FROM   OBLIGACION_TRANSACCION OT2
                INNER JOIN   TRANSACCION T2 ON OT2.ID_TRANSACCION = T2.ID_TRANSACCION
                WHERE  OT2.ID_OBLIGACION = O.ID_OBLIGACION
            )                                               AS FECHA_ULTIMO_PAGO,
            O.FECHA_INICIO,
            O.FECHA_FIN
        FROM OBLIGACION_FIJA O
        INNER JOIN SUBCATEGORIA    S ON O.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
        INNER JOIN CATEGORIA       C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
        WHERE O.ID_USER = P_ID_USER
          AND O.VIGENTE = 1
        ORDER BY O.DIA;
END SP_REPORTE_5;
/


-- ============================================================
-- REPORTE 6: Progreso de Metas de Ahorro
-- ============================================================
CREATE OR REPLACE PROCEDURE SP_REPORTE_6 (
    P_ID_USER       IN  USUARIO.ID_USER%TYPE,
    P_ID_PRESUPUESTO IN PRESUPUESTO.ID_PRESUPUESTO%TYPE,
    P_CURSOR        OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN P_CURSOR FOR
        SELECT
            S.ID_SUBCATEGORIA,
            S.NOMBRE                                        AS NOMBRE_META,
            C.NOMBRE                                        AS NOMBRE_CATEGORIA,
            D.MONTO                                         AS MONTO_OBJETIVO_MENSUAL,
            -- Meses de vigencia del presupuesto
            ( (P.ANIO_FIN  - P.ANIO_INICIO) * 12 +
              (P.MES_FIN   - P.MES_INICIO)  + 1 )          AS MESES_VIGENCIA,
            -- Objetivo total del período
            D.MONTO * ( (P.ANIO_FIN  - P.ANIO_INICIO) * 12 +
                        (P.MES_FIN   - P.MES_INICIO)  + 1 ) AS MONTO_OBJETIVO_TOTAL,
            -- Monto acumulado real
            NVL((
                SELECT SUM(T.MONTO)
                FROM   TRANSACCION T
                WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                  AND  T.ID_PRESUPUESTO  = P_ID_PRESUPUESTO
                  AND  T.TIPO            = 'ahorro'
                  AND  T.ID_USER         = P_ID_USER
            ), 0)                                           AS MONTO_ACUMULADO,
            -- Porcentaje completado
            ROUND(NVL((
                SELECT SUM(T.MONTO)
                FROM   TRANSACCION T
                WHERE  T.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
                  AND  T.ID_PRESUPUESTO  = P_ID_PRESUPUESTO
                  AND  T.TIPO            = 'ahorro'
                  AND  T.ID_USER         = P_ID_USER
            ), 0) * 100.0 / NULLIF(
                D.MONTO * ( (P.ANIO_FIN  - P.ANIO_INICIO) * 12 +
                            (P.MES_FIN   - P.MES_INICIO)  + 1 ), 0
            ), 2)                                           AS PORCENTAJE_COMPLETADO,
            -- Días restantes hasta fin del presupuesto
            (TO_DATE(P.ANIO_FIN || '-' || LPAD(P.MES_FIN,2,'0') || '-28', 'YYYY-MM-DD')
             - SYSDATE)                                     AS DIAS_RESTANTES,
            P.ANIO_FIN,
            P.MES_FIN
        FROM PRESUPUESTO_DETALLE D
        INNER JOIN SUBCATEGORIA        S ON D.ID_SUBCATEGORIA = S.ID_SUBCATEGORIA
        INNER JOIN CATEGORIA           C ON S.ID_CATEGORIA    = C.ID_CATEGORIA
        INNER JOIN PRESUPUESTO         P ON D.ID_PRESUPUESTO  = P.ID_PRESUPUESTO
        WHERE D.ID_PRESUPUESTO = P_ID_PRESUPUESTO
          AND C.TIPO           = 'ahorro'
        ORDER BY PORCENTAJE_COMPLETADO DESC;
END SP_REPORTE_6;
/
