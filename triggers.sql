-- ============================================================
-- SISTEMA DE PRESUPUESTO PERSONAL
-- Steve Valladares - 22341344
-- ============================================================

CREATE OR REPLACE TRIGGER TRG_SUBCAT_DEFECTO
AFTER INSERT ON CATEGORIA
FOR EACH ROW
DECLARE
    V_ID_SUBCAT VARCHAR2(10);
BEGIN
    V_ID_SUBCAT := 'S' || :NEW.ID_CATEGORIA;

    INSERT INTO SUBCATEGORIA (
        ID_SUBCATEGORIA,
        ID_CATEGORIA,
        NOMBRE,
        DESCRIPCION,
        ACTIVA,
        SUB_POR_DEFECTO
    ) VALUES (
        V_ID_SUBCAT,
        :NEW.ID_CATEGORIA,
        'General',
        'Subcategoría por defecto de ' || :NEW.NOMBRE,
        1,
        1
    );
END TRG_SUBCAT_DEFECTO;
/



-----------------------
CREATE OR REPLACE TRIGGER TRG_VALIDAR_PERIODO_TRANSACCION
BEFORE INSERT ON TRANSACCION
FOR EACH ROW
DECLARE
    V_RESULTADO NUMBER;
BEGIN
    V_RESULTADO := FN_VALIDAR_VIGENCIA(
        :NEW.ANIO,
        :NEW.MES,
        :NEW.ID_PRESUPUESTO
    );

    IF V_RESULTADO = 0 THEN
        RAISE_APPLICATION_ERROR(-20030,
            'El período ' || :NEW.ANIO || '/' || :NEW.MES ||
            ' está fuera del rango de vigencia del presupuesto.');
    END IF;
END TRG_VALIDAR_PERIODO_TRANSACCION;
/


-----------------------
CREATE OR REPLACE TRIGGER TRG_VALIDAR_TIPO_TRANSACCION
BEFORE INSERT ON TRANSACCION
FOR EACH ROW
DECLARE
    V_TIPO_SUBCAT VARCHAR2(10);
BEGIN
    SELECT C.TIPO INTO V_TIPO_SUBCAT
    FROM   SUBCATEGORIA S
    JOIN   CATEGORIA    C ON S.ID_CATEGORIA = C.ID_CATEGORIA
    WHERE  S.ID_SUBCATEGORIA = :NEW.ID_SUBCATEGORIA;

    IF V_TIPO_SUBCAT != :NEW.TIPO THEN
        RAISE_APPLICATION_ERROR(-20031,
            'El tipo "' || :NEW.TIPO || '" no coincide con ' ||
            'el tipo de la subcategoría "' || V_TIPO_SUBCAT || '".');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20002,
            'No se encontró la subcategoría: ' || :NEW.ID_SUBCATEGORIA);
END TRG_VALIDAR_TIPO_TRANSACCION;
/
