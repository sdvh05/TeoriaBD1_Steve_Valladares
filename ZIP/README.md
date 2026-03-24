📁 Proyecto: Presupuesto Personal (C# WinForms)
Este archivo contiene el código fuente completo de una aplicación de escritorio para gestión financiera personal. El proyecto fue desarrollado en C# con Windows Forms y utiliza Oracle Database como motor de datos, además de iTextSharp para la generación de reportes en PDF.


📋 Contenido del ZIP
TODO EL CODIGO FUENTE
(No se pudieron subir los packages), pero esos los instala cada quien con los requisitos

🖥️ Requisitos para compilar
1. Software necesario
Componente	Versión / Notas
Visual Studio	2022 (Community, Professional o Enterprise) con la carga de trabajo Desarrollo de escritorio .NET
.NET Framework	4.7.2 o superior (el proyecto apunta a .NET Framework 4.7.2)
Oracle Database	11g/12c/18c/19c / XE (instalado localmente o accesible en red)
Oracle Client	No estrictamente necesario si se usa Oracle.ManagedDataAccess (incluido vía NuGet)
2. Extensiones de Visual Studio (opcionales)
No se requieren extensiones específicas. Sin embargo, si deseas trabajar con Oracle directamente desde Visual Studio, puedes instalar:

TIP:
una vez dentro de Visual Studio hacer "Unload" al proyecto con los sql, para poder compilar la interfaz grafica

Oracle Developer Tools for Visual Studio (opcional, no necesario para compilar)

📦 Paquetes NuGet requeridos
El proyecto hace uso de los siguientes paquetes NuGet. Se restauran automáticamente al compilar si la opción está activada en Visual Studio.

Paquete	Versión sugerida	Uso
Oracle.ManagedDataAccess	21.13.0 o superior	Conector ADO.NET para Oracle (manejado)
iTextSharp	5.5.13.3 o superior	Generación de reportes PDF
Si no se restauran automáticamente, abre la Consola del Administrador de paquetes y ejecuta:

powershell
Install-Package Oracle.ManagedDataAccess
Install-Package iTextSharp
