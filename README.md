# 💰 Sistema de Presupuesto Personal

> **Taller de Base de Datos I — Proyecto Final**  
> Steve Valladares · 22341344

Sistema de gestión de presupuesto personal que permite planificar, controlar y analizar finanzas personales mediante una aplicación de escritorio conectada a Oracle Database.

---

## 📋 Descripción General

El sistema permite a múltiples usuarios:

- Definir presupuestos con vigencia temporal (por mes, trimestre, semestre o año)
- Clasificar ingresos, gastos y ahorros mediante categorías y subcategorías
- Registrar obligaciones fijas mensuales (alquiler, servicios, deudas)
- Registrar transacciones individuales clasificadas por categoría
- Visualizar 6 reportes analíticos con exportación a PDF

---

## 🏗️ Arquitectura del Sistema

El sistema sigue una **arquitectura de 3 capas**:

```
┌─────────────────────────────┐
│   CAPA PRESENTACIÓN         │  Windows Forms (C#)
│   Frontend / WinForms       │  FrmLogin, FrmMenu, FrmTransacciones...
└──────────────┬──────────────┘
               │
┌──────────────▼──────────────┐
│   CAPA DE NEGOCIO           │  DAOs + OracleHelper (C#)
│   Backend                   │  UsuarioDAO, TransaccionDAO, ReporteDAO...
└──────────────┬──────────────┘
               │
┌──────────────▼──────────────┐
│   CAPA DE DATOS             │  Oracle Database 21c XE
│   Base de Datos             │  Tablas, SPs, Funciones, Triggers
└─────────────────────────────┘
```

> **Importante:** Toda la lógica de negocio reside en la base de datos mediante procedimientos almacenados. El frontend solo llama SPs y muestra resultados — no ejecuta SQL directo.

---

## 🛠️ Tecnologías

| Capa | Tecnología | Versión |
|---|---|---|
| Base de Datos | Oracle Database XE | 21c |
| Lenguaje BD | PL/SQL | — |
| Aplicación | C# / .NET Framework | 4.7.2 |
| UI | Windows Forms | — |
| Driver BD | Oracle.ManagedDataAccess | 23.x |
| Exportación | iTextSharp | 5.5.13 |
| IDE | Visual Studio | 2022 |
| Gestor BD | SQL Developer | 24.3 |

---

## 📁 Estructura del Repositorio

```
TeoriaBD1_Steve_Valladares/
├── README.md
├── database/
│   ├── DDL/
│   │   └── 01_crear_tablas.sql        # Tablas, sequences, índices
│   ├── procedimientos/
│   │   ├── crud_usuario.sql
│   │   ├── crud_categoria.sql
│   │   ├── crud_subcategoria.sql
│   │   ├── crud_presupuesto.sql
│   │   ├── crud_presupuesto_detalle.sql
│   │   ├── crud_transaccion.sql
│   │   ├── crud_obligacion.sql
│   │   ├── procedures_negocio.sql     # SPs de lógica de negocio
│   │   └── Reportes.sql               # SPs para los 6 reportes
│   ├── funciones/
│   │   └── funciones.sql              # 10 funciones PL/SQL
│   ├── triggers/
│   │   └── triggers.sql               # Triggers obligatorios
│   └── datos_prueba/
│       └── insertar_datos.sql         # Dataset USR001 — 2 meses completos
├── frontend/
│   └── TBD1_App/
│       ├── Backend/                   # DAOs y OracleHelper
│       ├── Frontend/                  # Windows Forms
│       ├── Models/                    # Clases del dominio
│       └── README.md
└── docs/
    └── DBML_TBD1_22341344.dbml        # Modelo de datos
```

---

## 🗄️ Modelo de Datos

| Tabla | Descripción |
|---|---|
| `USUARIO` | Usuarios del sistema con salario base |
| `CATEGORIA` | Clasificación principal (ingreso/gasto/ahorro) |
| `SUBCATEGORIA` | Clasificación secundaria por categoría |
| `PRESUPUESTO` | Plan financiero por período de tiempo |
| `PRESUPUESTO_DETALLE` | Monto mensual planeado por subcategoría |
| `TRANSACCION` | Movimientos financieros reales |
| `OBLIGACION_FIJA` | Pagos recurrentes mensuales |
| `OBLIGACION_TRANSACCION` | Relación obligación-transacción pagada |

**IDs automáticos** generados por triggers: `USR001`, `CAT001`, `SUB001`, `PRE001`, `DET001`, `OBL001`, `TRX001`

---

## ⚙️ Instalación y Configuración

### Requisitos previos

- Oracle Database 21c XE — [Descargar](https://www.oracle.com/database/technologies/xe-downloads.html)
- SQL Developer 24.x — [Descargar](https://www.oracle.com/tools/downloads/sqldev-downloads.html)
- Visual Studio 2022
- .NET Framework 4.7.2

### 1. Configurar Oracle

```bash
# Si Oracle no inicia después de reiniciar:
lsnrctl start

sqlplus / as sysdba
ALTER SYSTEM REGISTER;
EXIT;
```

Datos de conexión:
```
Host:     localhost
Puerto:   1521
SID:      XE
Usuario:  system
Password: OracleTBD
```

### 2. Ejecutar scripts SQL en este orden

```
1. database/DDL/01_crear_tablas.sql
2. database/funciones/funciones.sql
3. database/triggers/triggers.sql
4. database/procedimientos/crud_usuario.sql
5. database/procedimientos/crud_categoria.sql
6. database/procedimientos/crud_subcategoria.sql
7. database/procedimientos/crud_presupuesto.sql
8. database/procedimientos/crud_presupuesto_detalle.sql
9. database/procedimientos/crud_transaccion.sql
10. database/procedimientos/crud_obligacion.sql
11. database/procedimientos/procedures_negocio.sql
12. database/procedimientos/Reportes.sql
13. database/datos_prueba/insertar_datos.sql   ← opcional
```

### 3. Ejecutar la aplicación

```
1. Abrir frontend/TBD1_App/TBD1_App.sln en Visual Studio 2022
2. Clic derecho en TBD1_App → Restore NuGet Packages
3. Ctrl+Shift+B para compilar
4. F5 para ejecutar
```

### 4. Credenciales de prueba

```
Correo:   sdvh05@gmail.com
Password: 1234
```

---

## 📊 Módulos del Sistema

| Módulo | Funcionalidad |
|---|---|
| Login / Registro | Autenticación multi-usuario |
| Menú Principal | Dashboard con balance del mes actual |
| Presupuestos | CRUD + detalles por subcategoría |
| Transacciones | Registro con filtros por presupuesto, año, mes y tipo |
| Categorías | CRUD de categorías y subcategorías |
| Obligaciones | Registro con indicadores de urgencia por días restantes |
| Mi Perfil | Ver y editar información personal |
| Reportes | 6 reportes con gráficas y exportación PDF |

---

## 📈 Reportes Implementados

| # | Reporte | Visualización |
|---|---|---|
| R1 | Resumen Mensual Ingresos vs Gastos vs Ahorros | Gráfica de barras + tabla |
| R2 | Distribución de Gastos por Categoría | Gráfica circular (pie) + tabla |
| R3 | Cumplimiento de Presupuesto por Subcategoría | Tabla con semáforo verde/amarillo/rojo |
| R4 | Tendencia de Gastos en el Tiempo | Gráfica de líneas múltiples |
| R5 | Estado de Obligaciones Fijas | Tabla con indicadores de estado |
| R6 | Progreso de Metas de Ahorro | Barras de progreso + tabla |

---

## 👤 Autor

**Steve Valladares** · 22341344  
Taller de Base de Datos I
