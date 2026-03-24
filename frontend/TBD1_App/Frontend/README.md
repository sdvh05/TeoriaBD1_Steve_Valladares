💰 Presupuesto Personal — Sistema de Gestión Financiera
Aplicación de escritorio desarrollada en C# WinForms para la gestión de finanzas personales. Permite a los usuarios registrar ingresos, gastos, ahorros, establecer presupuestos por períodos, gestionar obligaciones fijas, generar reportes analíticos y visualizar el cumplimiento de metas financieras.

Proyecto académico para la asignatura Teoría de Bases de Datos I.

🚀 Características principales
Autenticación de usuarios
Registro e inicio de sesión con validación de credenciales.

Gestión de presupuestos
Creación de presupuestos por períodos (anual/mensual) con detalles por subcategoría.

Transacciones
Registro de ingresos, gastos y ahorros con asignación a subcategorías, fecha, método de pago, número de factura y observaciones.

Categorías y subcategorías
Administración de categorías (ingreso, gasto, ahorro) y subcategorías asociadas.

Obligaciones fijas
Control de pagos recurrentes mensuales con alertas de vencimiento y estado.

Perfil de usuario
Edición de datos personales y salario base.

Reportes analíticos
6 reportes visuales con gráficos interactivos y exportación a PDF mediante iTextSharp:

Balance mensual (barras)
Gastos por categoría (gráfico circular)
Cumplimiento de presupuesto (semáforo)
Tendencia de gastos (líneas)
Estado de obligaciones (tabla con alertas)
Progreso de ahorro (barras de progreso)
Interfaz moderna
Tema oscuro, controles personalizados, bordes redondeados y efectos hover.

🛠️ Tecnologías utilizadas
Tecnología	Uso
C# .NET Framework	Lenguaje principal
Windows Forms	Interfaz gráfica de usuario
Oracle Database	Almacenamiento de datos
Oracle.ManagedDataAccess	Conector ADO.NET para Oracle
iTextSharp	Exportación de reportes a PDF
GDI+	Dibujo personalizado de gráficos y UI


📋 Requisitos previos
Sistema operativo: Windows 7 o superior

.NET Framework: 4.7.2 o superior

Oracle Database: 11g/12c/18c/19c o XE

Oracle Client: Instalado (opcional si se usa ODAC)

Visual Studio: 2019/2022 (para compilar)

🗄️ Configuración de la base de datos
Crear un usuario/tablaspace en Oracle.

Ejecutar los scripts DDL (no incluidos en este repositorio, pero se esperan las siguientes tablas principales):

USUARIO

CATEGORIA

SUBCATEGORIA

PRESUPUESTO

PRESUPUESTO_DETALLE

TRANSACCION

OBLIGACION_FIJA

HISTORIAL_PAGO (opcional)

Ajustar la cadena de conexión en el archivo de configuración (ej. App.config):

xml
<connectionStrings>
  <add name="OracleDbContext" 
       connectionString="Data Source=localhost:1521/XE;User Id=TU_USUARIO;Password=TU_PASSWORD;" 
       providerName="Oracle.ManagedDataAccess.Client"/>
</connectionStrings>
El acceso a datos se realiza mediante clases DAO que utilizan OracleHelper (clase estática para ejecutar consultas).

▶️ Ejecutar la aplicación
Abrir la solución en Visual Studio.

Asegurar que todas las referencias NuGet están restauradas (Oracle.ManagedDataAccess, iTextSharp).

Compilar y ejecutar (F5).

La ventana de inicio de sesión aparecerá. Para usuarios nuevos, hacer clic en "¿No tienes cuenta? Crear cuenta nueva".

📁 Estructura del proyecto (resumen)
text
TBD1_App/
├── Frontend/
│   ├── FrmLogin.cs               # Inicio de sesión
│   ├── FrmRegistro.cs            # Registro de nuevos usuarios
│   ├── FrmMenu.cs                # Menú principal con resumen
│   ├── FrmCategorias.cs          # Gestión de categorías y subcategorías
│   ├── FrmTransacciones.cs       # Listado y registro de transacciones
│   ├── FrmTransaccionDetalle.cs  # Diálogo para nueva/editar transacción
│   ├── FrmPresupuestos.cs        # Gestión de presupuestos y detalles
│   ├── FrmObligaciones.cs        # Obligaciones fijas
│   ├── FrmPerfil.cs              # Edición de perfil de usuario
│   └── FrmReportes.cs            # Reportes con gráficos y exportación PDF



👤 Autor
Steve Valladares – Desarrollador
Proyecto académico – Teoría de Bases de Datos I
Universidad Tecnológica de Honduras (UTH)

📄 Licencia
Este proyecto es de uso académico. No se otorga licencia para fines comerciales.
