# Sistema de Presupuesto Personal
**Stack:** Oracle DB + C# Windows Forms (Visual Studio)

---

## 📁 Estructura de archivos entregados

```
📂 SQL/
  01_DDL_crear_tablas.sql          ← Crear todas las tablas + triggers
  02_procedimientos_y_funciones.sql ← Stored procedures y funciones
  03_datos_prueba.sql              ← Datos de prueba (2 meses)

📂 CSharp/
  OracleHelper.cs       ← Conexión a Oracle (clase estática reutilizable)
  FrmDashboard.cs       ← Pantalla principal con resumen del mes
  FrmUsuarios.cs        ← CRUD completo de usuarios
  FrmCategorias.cs      ← CRUD de categorías + subcategorías
  FrmTransacciones.cs   ← Registrar y listar transacciones
```

---

## 🚀 Pasos para configurar

### 1. Base de Datos Oracle

1. Abre **SQL Developer** y conéctate a tu Oracle XE
2. Ejecuta los scripts **en este orden**:
   ```
   01_DDL_crear_tablas.sql
   02_procedimientos_y_funciones.sql
   03_datos_prueba.sql
   ```

### 2. Proyecto C# en Visual Studio

1. Crea un nuevo proyecto: **Windows Forms App (.NET)**
   - Nombre: `PresupuestoPersonal`
2. Instala el paquete NuGet de Oracle:
   - Ve a **Tools → NuGet Package Manager → Manage NuGet Packages**
   - Busca e instala: `Oracle.ManagedDataAccess.Core`
3. Copia los archivos `.cs` al proyecto
4. **Actualiza la cadena de conexión** en `OracleHelper.cs`:
   ```csharp
   "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)
   (HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=XEPDB1)));
   User Id=TU_USUARIO;Password=TU_PASSWORD;"
   ```
   - `SERVICE_NAME`: puede ser `XE`, `XEPDB1` o el nombre de tu BD
   - `User Id` / `Password`: tus credenciales Oracle

### 3. Diseñar los formularios (Designer)

Para cada Form, necesitas agregar controles en el **Windows Forms Designer**:

#### FrmDashboard
- 4x `Label` (lblIngresos, lblGastos, lblAhorros, lblBalance)
- 4x `Button` (btnUsuarios, btnCategorias, btnTransacciones, btnPresupuestos, btnActualizar)

#### FrmUsuarios
- `DataGridView` → `dgvUsuarios`
- `TextBox` → `txtNombre`, `txtApellido`, `txtEmail`, `txtSalario`
- `Button` → `btnGuardar`, `btnActualizar`, `btnEliminar`, `btnLimpiar`

#### FrmCategorias
- `DataGridView` → `dgvCategorias`, `dgvSubcategorias`
- `TextBox` → `txtNombre`, `txtDescripcion`, `txtNombreSubcat`
- `ComboBox` → `cmbTipo`
- `Button` → `btnGuardarCategoria`, `btnAgregarSubcat`, `btnRefrescar`

#### FrmTransacciones
- `DataGridView` → `dgvTransacciones`
- `ComboBox` → `cmbUsuario`, `cmbPresupuesto`, `cmbSubcategoria`, `cmbTipo`, `cmbMetodoPago`
- `TextBox` → `txtDescripcion`, `txtMonto`
- `NumericUpDown` → `numAnio`, `numMes`
- `DateTimePicker` → `dtpFecha`
- `Button` → `btnGuardar`, `btnEliminar`, `btnLimpiar`, `btnRefrescar`

---

## 💡 Tips Oracle XE

- Si usas Oracle XE local, el SERVICE_NAME suele ser `XEPDB1` o `XE`
- Puerto por defecto: `1521`
- Para ver tus servicios: ejecuta en SQL*Plus:
  ```sql
  SELECT VALUE FROM V$PARAMETER WHERE NAME = 'service_names';
  ```

---

## 📋 Fases del proyecto

| Fase | Entregable | Estado |
|------|-----------|--------|
| 1 - Documentación | Modelo relacional | 🔲 Pendiente (usa los SQL generados) |
| 2 - BD | DDL + Procedures + Triggers + Datos | ✅ Scripts listos |
| 3 - App | Frontend C# + Backend | ✅ Base lista |
| 4 - Reportería | 6 reportes en PDF | 🔲 Pendiente |
| 5 - Entrega Final | Repo + Video + PPT | 🔲 Pendiente |
