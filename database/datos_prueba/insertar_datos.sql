SET SERVEROUTPUT ON;


DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT003','Alquiler','Pago mensual de alquiler',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT003','Agua','Factura de agua',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT003','Luz','Factura de electricidad',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT003','Internet','Servicio de internet',V_ID);
END;
/

-- >> Alimentacion (CAT004)
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT004','Supermercado','Compras de despensa',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT004','Restaurantes','Comidas fuera de casa',V_ID);
END;
/

-- >> Transporte (CAT005)
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT005','Bus/Urbano','Transporte publico',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT005','Gasolina','Combustible para vehiculo',V_ID);
END;
/

-- >> Salud (CAT006)
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT006','Medicamentos','Compra de medicamentos',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT006','Consultas','Consultas medicas',V_ID);
END;
/

-- >> Entretenimiento (CAT008)
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT008','Streaming','Netflix, Spotify, etc.',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT008','Salidas','Cines, eventos, salidas',V_ID);
END;
/

-- >> Ahorro (CAT009)
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT009','Emergencia','Fondo de emergencia',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_SUBCATEGORIA('CAT009','Meta viaje','Ahorro para vacaciones',V_ID);
END;
/

-- Verificar IDs generados
SELECT ID_SUBCATEGORIA, ID_CATEGORIA, NOMBRE FROM SUBCATEGORIA ORDER BY ID_SUBCATEGORIA;

-- ============================================================
-- PRESUPUESTO Enero-Febrero 2025
-- ============================================================

-- >> PRE001
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_PRESUPUESTO(
        P_ID_USER            => 'USR001',
        P_NOMBRE_DESCRIPTIVO => 'Presupuesto Ene-Feb 2025',
        P_ANIO_INICIO        => 2025,
        P_MES_INICIO         => 1,
        P_ANIO_FIN           => 2025,
        P_MES_FIN            => 2,
        P_ID_GENERADO        => V_ID
    );
    DBMS_OUTPUT.PUT_LINE('Presupuesto creado: ' || V_ID);
END;
/

-- ============================================================
-- PRESUPUESTO_DETALLE (montos planificados)
-- SUB001 = General Sueldo (ingreso)
-- SUB010 = Alquiler, SUB011 = Agua, SUB012 = Luz, SUB013 = Internet
-- SUB014 = Supermercado, SUB015 = Restaurantes
-- SUB016 = Bus/Urbano, SUB017 = Gasolina
-- SUB018 = Medicamentos, SUB019 = Consultas
-- SUB020 = Streaming, SUB021 = Salidas
-- SUB022 = Emergencia, SUB023 = Meta viaje
-- ============================================================

-- >> Ingresos planificados
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB001',18000,'Sueldo mensual esperado',V_ID);
END;
/

-- >> Gastos planificados
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB010',5500,'Alquiler mensual',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB011',350,'Agua estimada',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB012',600,'Luz estimada',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB013',500,'Internet mensual',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB014',2500,'Supermercado mensual',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB015',800,'Restaurantes y salidas a comer',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB016',400,'Bus urbano mensual',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB018',500,'Medicamentos preventivos',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB020',300,'Netflix y Spotify',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB021',600,'Salidas y entretenimiento',V_ID);
END;
/

-- >> Ahorros planificados
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB022',2000,'Fondo de emergencia',V_ID);
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_DETALLE('PRE001','SUB023',1000,'Meta viaje fin de anno',V_ID);
END;
/

-- ============================================================
-- OBLIGACIONES FIJAS
-- ============================================================

-- >> OBL001 - Alquiler
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_OBLIGACION(
        'USR001','SUB010','Alquiler Apartamento',
        'Pago fijo mensual de vivienda',
        5500,1,DATE '2024-01-01',NULL,V_ID
    );
    DBMS_OUTPUT.PUT_LINE('Obligacion: ' || V_ID);
END;
/

-- >> OBL002 - Internet
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_OBLIGACION(
        'USR001','SUB013','Internet Tigo',
        'Plan hogar 200Mbps',
        500,5,DATE '2024-01-01',NULL,V_ID
    );
    DBMS_OUTPUT.PUT_LINE('Obligacion: ' || V_ID);
END;
/

-- >> OBL003 - Streaming
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_OBLIGACION(
        'USR001','SUB020','Netflix + Spotify',
        'Suscripciones streaming',
        300,10,DATE '2024-01-01',NULL,V_ID
    );
    DBMS_OUTPUT.PUT_LINE('Obligacion: ' || V_ID);
END;
/

-- >> OBL004 - Agua
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_OBLIGACION(
        'USR001','SUB011','Agua SANAA',
        'Factura mensual agua',
        350,15,DATE '2024-01-01',NULL,V_ID
    );
    DBMS_OUTPUT.PUT_LINE('Obligacion: ' || V_ID);
END;
/

-- >> OBL005 - Luz
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_OBLIGACION(
        'USR001','SUB012','Luz ENEE',
        'Factura mensual electricidad',
        600,20,DATE '2024-01-01',NULL,V_ID
    );
    DBMS_OUTPUT.PUT_LINE('Obligacion: ' || V_ID);
END;
/

-- ============================================================
-- TRANSACCIONES — ENERO 2025
-- ============================================================

-- >> Ingresos
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB001',2025,1,'ingreso',
        'Sueldo enero',18000,DATE '2025-01-01',
        'transferencia',NULL,NULL,V_ID
    );
END;
/

-- >> Vivienda
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB010',2025,1,'gasto',
        'Alquiler enero',5500,DATE '2025-01-01',
        'transferencia','F-001',NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB013',2025,1,'gasto',
        'Internet Tigo enero',500,DATE '2025-01-05',
        'tarjeta_debito','F-002',NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB011',2025,1,'gasto',
        'Agua enero',320,DATE '2025-01-15',
        'efectivo','F-003',NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB012',2025,1,'gasto',
        'Luz enero',580,DATE '2025-01-20',
        'efectivo','F-004',NULL,V_ID
    );
END;
/

-- >> Alimentacion
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,1,'gasto',
        'Supermercado semana 1',650,DATE '2025-01-06',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,1,'gasto',
        'Supermercado semana 2',590,DATE '2025-01-13',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,1,'gasto',
        'Supermercado semana 3',620,DATE '2025-01-20',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,1,'gasto',
        'Supermercado semana 4',540,DATE '2025-01-27',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB015',2025,1,'gasto',
        'Almuerzo con companeros',180,DATE '2025-01-10',
        'efectivo',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB015',2025,1,'gasto',
        'Cena familiar restaurante',350,DATE '2025-01-18',
        'tarjeta_credito',NULL,NULL,V_ID
    );
END;
/

-- >> Transporte
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB016',2025,1,'gasto',
        'Bus urbano enero',380,DATE '2025-01-31',
        'efectivo',NULL,'Pase mensual',V_ID
    );
END;
/

-- >> Salud
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB018',2025,1,'gasto',
        'Medicamentos gripe',220,DATE '2025-01-08',
        'efectivo','F-010',NULL,V_ID
    );
END;
/

-- >> Entretenimiento
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB020',2025,1,'gasto',
        'Netflix + Spotify enero',300,DATE '2025-01-10',
        'tarjeta_credito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB021',2025,1,'gasto',
        'Cine con amigos',150,DATE '2025-01-25',
        'efectivo',NULL,NULL,V_ID
    );
END;
/

-- >> Ahorro
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB022',2025,1,'ahorro',
        'Fondo emergencia enero',2000,DATE '2025-01-31',
        'transferencia',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB023',2025,1,'ahorro',
        'Meta viaje enero',1000,DATE '2025-01-31',
        'transferencia',NULL,NULL,V_ID
    );
END;
/

-- ============================================================
-- TRANSACCIONES — FEBRERO 2025
-- ============================================================

-- >> Ingresos
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB001',2025,2,'ingreso',
        'Sueldo febrero',18000,DATE '2025-02-01',
        'transferencia',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB002',2025,2,'ingreso',
        'Pago proyecto freelance',3500,DATE '2025-02-14',
        'transferencia',NULL,'Diseno web cliente',V_ID
    );
END;
/

-- >> Vivienda
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB010',2025,2,'gasto',
        'Alquiler febrero',5500,DATE '2025-02-01',
        'transferencia','F-020',NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB013',2025,2,'gasto',
        'Internet Tigo febrero',500,DATE '2025-02-05',
        'tarjeta_debito','F-021',NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB011',2025,2,'gasto',
        'Agua febrero',290,DATE '2025-02-15',
        'efectivo','F-022',NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB012',2025,2,'gasto',
        'Luz febrero',640,DATE '2025-02-20',
        'efectivo','F-023',NULL,V_ID
    );
END;
/

-- >> Alimentacion
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,2,'gasto',
        'Supermercado semana 1',700,DATE '2025-02-03',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,2,'gasto',
        'Supermercado semana 2',480,DATE '2025-02-10',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,2,'gasto',
        'Supermercado semana 3',560,DATE '2025-02-17',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB014',2025,2,'gasto',
        'Supermercado semana 4',490,DATE '2025-02-24',
        'tarjeta_debito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB015',2025,2,'gasto',
        'Cena San Valentin',520,DATE '2025-02-14',
        'tarjeta_credito',NULL,'Cena romantica',V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB015',2025,2,'gasto',
        'Almuerzo trabajo',140,DATE '2025-02-21',
        'efectivo',NULL,NULL,V_ID
    );
END;
/

-- >> Transporte
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB016',2025,2,'gasto',
        'Bus urbano febrero',380,DATE '2025-02-28',
        'efectivo',NULL,'Pase mensual',V_ID
    );
END;
/

-- >> Salud
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB019',2025,2,'gasto',
        'Consulta medica general',450,DATE '2025-02-12',
        'tarjeta_debito','F-030',NULL,V_ID
    );
END;
/

-- >> Entretenimiento
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB020',2025,2,'gasto',
        'Netflix + Spotify febrero',300,DATE '2025-02-10',
        'tarjeta_credito',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB021',2025,2,'gasto',
        'Concierto boletos',400,DATE '2025-02-22',
        'tarjeta_credito',NULL,NULL,V_ID
    );
END;
/

-- >> Ahorro
-- ---------------------------------------------------------
DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB022',2025,2,'ahorro',
        'Fondo emergencia febrero',2000,DATE '2025-02-28',
        'transferencia',NULL,NULL,V_ID
    );
END;
/

DECLARE
    V_ID VARCHAR2(10);
BEGIN
    SP_INSERTAR_TRANSACCION(
        'USR001','PRE001','SUB023',2025,2,'ahorro',
        'Meta viaje febrero',1000,DATE '2025-02-28',
        'transferencia',NULL,NULL,V_ID
    );
END;
/

COMMIT;
