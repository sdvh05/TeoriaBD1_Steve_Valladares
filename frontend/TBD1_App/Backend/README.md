# 🔧 Backend — Sistema de Presupuesto Personal

Stack de acceso a datos: **C# + Oracle.ManagedDataAccess + PL/SQL**

---

## 📁 Estructura

```
TBD1_App/
├── Backend/
│   ├── OracleHelper.cs       ← Conexión centralizada a Oracle
│   ├── UsuarioDAO.cs         ← CRUD de usuarios
│   ├── CategoriaDAO.cs       ← CRUD de categorías y subcategorías
│   ├── TransaccionDAO.cs     ← Registro y consulta de transacciones
│   ├── PresupuestoDAO.cs     ← Gestión de presupuestos y detalles
│   ├── ObligacionDAO.cs      ← Gestión de obligaciones fijas
│   └── ReporteDAO.cs         ← Generación de los 6 reportes financieros
└── Models/
    ├── Usuario.cs
    ├── Categoria.cs
    ├── Subcategoria.cs
    ├── Presupuesto.cs
    ├── PresupuestoDetalle.cs
    ├── Transaccion.cs
    ├── ObligacionFija.cs
    └── ReporteModels.cs      ← 6 clases para los reportes
```

---

## 📦 Dependencias NuGet

| Paquete | Versión | Uso |
|---|---|---|
| `Oracle.ManagedDataAccess` | 23.x | Driver oficial Oracle para .NET Framework |
| `iTextSharp` | 5.5.13.5 | Exportación de reportes a PDF |
| `Microsoft.VisualBasic` | — | InputBox para renombrar presupuestos |

> **Oracle.ManagedDataAccess** es la versión "Managed" — no requiere instalar Oracle Client en la máquina. Todo el driver está contenido en el paquete NuGet.

---

## ⚡ OracleHelper — Clase central de conexión

Clase **estática** que centraliza toda comunicación con Oracle. Ningún DAO abre conexiones directamente — todo pasa por aquí.

### Cadena de conexión

```csharp
"Data Source=(DESCRIPTION=
    (ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))
    (CONNECT_DATA=(SERVICE_NAME=XE)));
User Id=system;
Password=OracleTBD;"
```

| Campo | Valor | Descripción |
|---|---|---|
| `HOST` | `localhost` | Oracle corre en la misma máquina |
| `PORT` | `1521` | Puerto estándar del listener Oracle |
| `SERVICE_NAME` | `XE` | Nombre de la instancia Express Edition |
| `User Id` | `system` | Usuario administrador Oracle |

### Métodos disponibles

```csharp
// SELECT — retorna múltiples filas como DataTable
OracleHelper.ExecuteQuery(sql, params)  →  DataTable

// SP sin retorno — INSERT, UPDATE, DELETE
OracleHelper.ExecuteProcedure(nombre, params)  →  void

// SP con parámetro OUT — retorna ID generado
OracleHelper.ExecuteProcedureWithOutput(nombre, params)  →  string

// Query de un solo valor — COUNT, SUM
OracleHelper.ExecuteScalar(sql, params)  →  object

// Verifica si Oracle está disponible
OracleHelper.TestConnection()  →  bool
```

### ¿Por qué `using`?

```csharp
using (var conn = OracleHelper.GetConnection())
using (var cmd  = new OracleCommand(sql, conn))
{
    // trabajo aquí
}   // ← conexión se cierra automáticamente aunque haya error
```

`using` implementa `IDisposable` — garantiza que la conexión se libera sin necesidad de llamar `.Close()` manualmente.

---

## 🗂️ Patrón DAO

Cada DAO sigue el mismo patrón de 3 pasos:

```
1. Llamar OracleHelper (ejecutar SP o query)
2. Recibir DataTable o parámetros OUT
3. Mapear filas → objetos C# con método privado Mapear...()
```

Ejemplo con `CategoriaDAO`:

```csharp
public List<Categoria> ObtenerTodas()
{
    // 1. Ejecutar query
    var dt = OracleHelper.ExecuteQuery(
        "SELECT * FROM CATEGORIA ORDER BY ORDEN, NOMBRE");

    // 2 + 3. Mapear cada fila a objeto
    var lista = new List<Categoria>();
    foreach (DataRow row in dt.Rows)
        lista.Add(MapearCategoria(row));

    return lista;
}

private Categoria MapearCategoria(DataRow row)
{
    return new Categoria
    {
        IdCategoria = row["ID_CATEGORIA"].ToString(),
        Nombre      = row["NOMBRE"].ToString(),
        // DBNull.Value = NULL de Oracle; hay que verificar antes de .ToString()
        Descripcion = row["DESCRIPCION"] == DBNull.Value
                      ? null
                      : row["DESCRIPCION"].ToString()
    };
}
```

---

## 🔄 Tipos de operaciones por DAO

### Insertar — retorna ID generado

```csharp
// El SP tiene un parámetro OUT P_ID_GENERADO
// El trigger en Oracle genera el ID automáticamente (USR001, USR002...)
string nuevoId = OracleHelper.ExecuteProcedureWithOutput(
    "SP_INSERTAR_USUARIO",
    new OracleParameter("P_PRIMER_NOMBRE", usuario.PrimerNombre),
    // ... más parámetros
    new OracleParameter("P_ID_GENERADO", OracleDbType.Varchar2, 10)
    { Direction = ParameterDirection.Output }
);
```

### Valores nulos — `DBNull.Value`

```csharp
// Si el campo puede ser null, hay que convertirlo
new OracleParameter("P_DESCRIPCION", (object)c.Descripcion ?? DBNull.Value)
// Si c.Descripcion es null en C# → manda DBNull.Value (NULL de Oracle)
// Si tiene valor → manda el valor
```

### Parámetros con `:nombre`

Oracle usa `:nombreParam` (con dos puntos), diferente a SQL Server (`@param`):
```sql
WHERE ID_USER = :u   AND   ANIO = :anio
```

---

## 📊 ReporteDAO — Caso especial con RefCursor

Los reportes usan `SYS_REFCURSOR` para retornar múltiples filas desde un SP. Esto requiere manejar la conexión manualmente porque el cursor necesita que la conexión esté abierta mientras se lee:

```csharp
var cursor = new OracleParameter("P_CURSOR", OracleDbType.RefCursor)
             { Direction = ParameterDirection.Output };

using (var conn = OracleHelper.GetConnection())
using (var cmd  = new OracleCommand("SP_REPORTE_1", conn))
{
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.Parameters.Add(cursor);
    cmd.ExecuteNonQuery();

    // Leer el cursor mientras la conexión sigue abierta
    using (var reader = ((OracleRefCursor)cursor.Value).GetDataReader())
        while (reader.Read())
            lista.Add(new ReporteBalanceMes {
                Anio = reader.GetInt32(0),
                // columnas por índice (0, 1, 2...) en el orden del SELECT
            });
}
```

---

## 📌 Notas importantes

**IDs automáticos** — generados por triggers en Oracle, no por el código C#. El SP inserta `NULL` y el trigger intercepta con `BEFORE INSERT` asignando el siguiente ID.

**Soft delete** — las eliminaciones no borran físicamente. Usan flags: `ACTIVA = 0` en subcategorías, `VIGENTE = 0` en obligaciones. Esto preserva el historial de transacciones.

**Sin SQL directo en Forms** — los Forms nunca llaman Oracle directamente. Toda query pasa por DAO → OracleHelper → Oracle. La única excepción es `FrmMenu` que usa `ExecuteQuery` para el balance general del mes.

**Transacciones de BD** — el COMMIT y ROLLBACK están en los SPs de Oracle, no en C#. Si el SP falla, Oracle hace ROLLBACK automáticamente.
