# Sistema de Presupuesto Personal - Backend

**Stack:** Oracle XE + C# Windows Forms (Visual Studio)

---

## 📁 Estructura de archivos entregados
📂 Backend/
OracleHelper.cs ← Conexión a Oracle (clase estática reutilizable)
UsuarioDAO.cs ← CRUD completo de usuarios
CategoriaDAO.cs ← CRUD de categorías + subcategorías
TransaccionDAO.cs ← Registrar y listar transacciones
PresupuestoDAO.cs ← Gestión de presupuestos
ObligacionDAO.cs ← Gestión de obligaciones fijas
ReporteDAO.cs ← Generación de reportes financieros

📂 Models/
(Clases de modelo: Usuario, Categoria, Subcategoria, Transaccion, etc.)

📂 Frontend/
FrmDashboard.cs ← Pantalla principal con resumen del mes
FrmUsuarios.cs ← CRUD completo de usuarios
FrmCategorias.cs ← CRUD de categorías + subcategorías
FrmTransacciones.cs ← Registrar y listar transacciones


---

## 🚀 Pasos para configurar

### 1. Base de Datos Oracle

1. Abre **SQL Developer** y conéctate a tu Oracle XE
2. Ejecuta los scripts **en este orden** (los scripts deben ser creados según el modelo):

### 2. Proyecto C# en Visual Studio

1. Crea un nuevo proyecto: **Windows Forms App (.NET Framework)** o **.NET Core/5+** con soporte Windows Forms.
- Nombre sugerido: `PresupuestoPersonal`
2. Instala el paquete NuGet de Oracle:
- Ve a **Tools → NuGet Package Manager → Manage NuGet Packages**
- Busca e instala: `Oracle.ManagedDataAccess` (para .NET Framework) o `Oracle.ManagedDataAccess.Core` (para .NET Core)
3. Copia todos los archivos `.cs` del backend y frontend a tu proyecto (organizados en carpetas si lo deseas).
4. **Actualiza la cadena de conexión** en `OracleHelper.cs` con tus credenciales:
```csharp
private static readonly string ConnectionString =
    "Data Source=(DESCRIPTION=" +
        "(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))" +
        "(CONNECT_DATA=(SERVICE_NAME=XE)));" +
    "User Id=TU_USUARIO;" +
    "Password=TU_PASSWORD;";

  ConnectionString REAL del Proyecto
    "Data Source=(DESCRIPTION=" +
    "(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))" +
    "(CONNECT_DATA=(SERVICE_NAME=XE)));" +
        "User Id=system;" +
        "Password=OracleTBD;"

    SERVICE_NAME: puede ser XE, XEPDB1 o el nombre de tu base de datos.

User Id / Password: tus credenciales Oracle.

💡 Tips Oracle XE
Si usas Oracle XE local, el SERVICE_NAME suele ser XEPDB1 (en versiones 18c/21c) o XE (en versiones 11g/12c).

Puerto por defecto: 1521

Para ver tus servicios, ejecuta en SQL*Plus o SQL Developer:
SELECT VALUE FROM V$PARAMETER WHERE NAME = 'service_names';

Si la conexión falla, verifica que el servicio Oracle esté activo:
lsnrctl status

📌 Notas sobre los DAOs
Cada clase DAO utiliza OracleHelper para conectarse y ejecutar procedimientos almacenados.

Los IDs son generados automáticamente por Oracle mediante secuencias y se devuelven como parámetros de salida.

Las operaciones de eliminación son lógicas (soft delete) mediante flags (ACTIVA, VIGENTE).

Para transacciones, se utiliza un Presupuesto como contexto principal; las transacciones se asocian a un presupuesto, año y mes.
