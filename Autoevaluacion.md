# 📝 Autoevaluación — Sistema de Presupuesto Personal

**Estudiante:** Steve Valladares · 22341344  
**Asignatura:** Teoría de Bases de Datos I  


---

## 1. Reflexión sobre el proceso de desarrollo

Este proyecto comenzó siendo bien desafiantes. No por su complejidad del todo, sino por que era algo novedoso, con lo que nunca habia trabajado: una base de datos nueva, un lenguaje de programación diferente al visto en clases previas, una arquitectura de capas, y una interfaz de usuario completa, todo conectado y funcionando en conjunto.

Al inicio, la sensación era de incertidumbre. No saber si lo que hacia iba bien, y aunque huvieron fallas, se lograban resolver mediante investigacion. Ademas fue limitante abstenerse de usar AI, para apoyarme en los SQL. Sin embargo, conforme el proyecto avanzó, empeze a sentir satisfacción de ver cómo iba funcionando el proyecto. Ver el menú principal cargando datos reales desde Oracle y todo funcionando, fue muy bonito

El resultado final es un sistema funcional de múltiples módulos, con lógica de negocio en la base de datos, interfaz de usuario funcional, reportes exportables a PDF.

---

## 2. Desafíos enfrentados y soluciones

### 2.1 El problema con Oracle desde el primer día

El primer obstáculo apareció antes de escribir una sola línea de código: la descarga de Oracle. Por error se descargó **Oracle Database Enterprise Edition** (versión de pago, para servidores empresariales) en lugar de **Oracle Database 21c XE** (Express Edition, gratuita). No note diferencia a primera vista — ambas se llaman "Oracle Database" — pero la Enterprise Edition requería licencia y una configuración de servidor que estaba fuera de mi alcance y el del proyecto.

Una vez instalado el XE correcto, apareció el siguiente desafío: el listener. Oracle no es una base de datos que simplemente "arranca" como MySQL. Tiene un proceso separado llamado listener que escucha conexiones en el puerto 1521, y aprender a manejarlo con `lsnrctl start` y `sqlplus / as sysdba` fue parte del aprendizaje. Cada reinicio del equipo significaba volver a levantar el servicio manualmente.

**Solución:** Documentar el proceso de arranque y tenerlo siempre a mano. Eventualmente se volvió rutina.

### 2.2 La sintaxis de Oracle vs lo visto en clase

En clase se trabajó con **PostgreSQL**, cuya sintaxis tiene diferencias importantes con Oracle/PL/SQL. Algunos de los choques más frecuentes fueron:

- En PostgreSQL los parámetros van con `$1`, `$2`. En Oracle van con `:nombre`
- `LIMIT 1` en PostgreSQL es `FETCH FIRST 1 ROWS ONLY` en Oracle
- Las secuencias se manejan diferente; en Oracle se usan triggers `BEFORE INSERT` para generar IDs automáticos
- Los tipos de datos varían: `SERIAL` de PostgreSQL no existe en Oracle, `BOOLEAN` tampoco — hay que usar `NUMBER(1)`


Además, la gestión de errores en PL/SQL con `RAISE_APPLICATION_ERROR` y bloques `EXCEPTION` fue completamente nueva. En SQL simple no existe ese nivel de control — Oracle me permitia poder definir códigos de error personalizados en el rango `-20000` a `-20999` y hacer que la base de datos valide reglas de negocio.

**Solución:** Consultar documentación de Oracle constantemente, comparar con lo conocido de PostgreSQL, y practicar con ejemplos pequeños antes de aplicar al proyecto.

### 2.3 Conectar C# con Oracle 

La decisión de usar **C# con Windows Forms** implicó aprender un lenguaje nuevo en paralelo al desarrollo del proyecto. Los primeros intentos de configurar el entorno fueron frustrantes:

- Se intentó trabajar en **CMD y PowerShell** con .NET CLI, lo cual funcionaba para proyectos simples pero se volvía complejo al agregar referencias a Oracle y manejar múltiples archivos
- Se probó **VS Code** con extensiones de C#, que mejoraba la experiencia pero aún tenía limitaciones para Windows Forms — el diseñador visual de formularios no funciona bien fuera de Visual Studio
- Finalmente se llegó a **Visual Studio Insiders 2026** (el IDE "morado"), que ofrece soporte nativo completo para Windows Forms, NuGet integrado, diseñador visual y depuración robusta

El driver de Oracle también generó confusión inicial. Existen múltiples paquetes: `Oracle.DataAccess` (requiere Oracle Client instalado), `Oracle.ManagedDataAccess` (puro .NET, sin dependencias externas) y `Oracle.ManagedDataAccess.Core` (para .NET Core). Elegir el correcto — `Oracle.ManagedDataAccess` para .NET Framework 4.7.2 — fue clave para que todo funcionara.

**Solución:** Establecer Visual Studio 2026 como entorno definitivo desde el principio y usar `Oracle.ManagedDataAccess` como driver único.

### 2.4 Excepciones y validaciones en la base de datos

Una de las decisiones de diseño más importantes — y más desafiantes — fue poner la lógica de negocio y las validaciones **dentro de los procedimientos almacenados**, no en el código C#. Esto significaba que si el usuario intentaba registrar una transacción fuera del período del presupuesto, era Oracle quien lanzaba el error, no C#, ni en front ni backend, todo sql.

Manejar esos errores correctamente requirió entender sintaxis de Oracle, y como se relacionaba con hasta la UI, cómo capturarlas en C# (Catch la exception) sin que la aplicación se cayera, y cómo mostrar mensajes útiles al usuario. Los bloques `EXCEPTION WHEN OTHERS THEN ROLLBACK; RAISE;` garantizan que si algo falla, la transacción se deshace completamente — asi no generando datos inconsistentes.

**Solución:** Adoptar el patrón de `try/catch` en todos los métodos del DAO, y mostrar el mensaje de Oracle directamente en la interfaz cuando contiene información útil para el usuario.

### 2.5 La reestructura del repositorio con Git Bash

El repositorio inició sin una estructura clara — todos los archivos estaban dispersos en la raíz. A mitad del proyecto fue necesario reorganizarlo en carpetas (`database/`, `frontend/`, `docs/`) para cumplir con los requisitos de entrega.

Hacer eso en Git sin perder el historial de commits requirió usar **Git Bash** para mover archivos con `mv` y `cp`, reindexar con `git add` y hacer commits descriptivos. Hubo un momento donde los triggers quedaron fuera del repositorio por error durante la reestructura — y tuvieron que ser recuperados y subidos por separado.

Ademas, previamente al tener trabajos individuales, habia acostumbrado a subir todo desde la rama Main, no usando branches como 'development' que use en este caso.

**Solución:** Aprender los comandos básicos de Git Bash (`cp -r`, `git add`, `git commit -m`, `git push`) y verificar siempre con `git status` antes de hacer commit.

---

## 3. Aprendizajes clave

**Oracle Database y PL/SQL**  
Aprender a trabajar con Oracle fue el aprendizaje más denso del proyecto. Entender la diferencia entre XE y las ediciones de pago, configurar el listener, escribir procedimientos almacenados con parámetros IN/OUT, manejar cursores/punteros REF CURSOR, crear triggers BEFORE INSERT para IDs automáticos, y diseñar funciones reutilizables como `FN_DIAS_HASTA_VENCIMIENTO` fueron habilidades completamente nuevas que ahora forman parte del repertorio técnico.

**Arquitectura de 3 capas**  
Previamente habiamos trabajado en 2 o 2.5 capas, usabamos solo front y backend, y los datos o se manejaban en arreglos que perdian la info al cerrarse la "run" del proyecto, o en archivos binarios, que funcionaba como un reemplazo simple y primitivo de la base de datos.

**C# y Windows Forms**  
Aprender C# en el contexto de un proyecto real aceleró el proceso de una manera que los ejercicios aislados no logran. Conceptos como `using` para manejo de recursos, `DBNull.Value` para nulos de Oracle. Me facilito muchisimo el uso de C# en VisualStudio por sobre Java en NetBeansIDE, y al ser orientado a objetos igual que java, se me facilto ese uso.
Tambien Ayudo que los Frms, y toda la UI/Frontend, si me pude apoyar de Inteligencia Artificial, porque ese pudo ser otro reto aun mayor, aprender a hacer UI sin AI en un lenguaje nuevo.

**Control de versiones con Git**  
Aprendi a manejar ramas, y a usar GitBash.

**Diseño de base de datos**  
Aprendimos a diseñar una base de datos, y la importancia del DBML, lo importante que son las relaciones, de llaves primarias, claves compuestas, e inclusive trucos "fuera de la teoria" (ejemplo Campaign) que nos facilitan el uso. Ademas ayudo que el Ing desarrollo el ER junto a nosotros, para evitar errores a la hora de conectar las tablas. 

---

## 4. Sugerencias de mejora

**Seguridad de contraseñas**  
Actualmente las contraseñas se almacenan en texto plano en la base de datos. Una mejora fundamental sería implementar hashing (BCrypt o SHA-256) antes de guardarlas. Esto es una limitación conocida del alcance académico del proyecto.

**Pago de obligaciones fijas**  
El módulo de obligaciones muestra el estado de cada pago (pendiente, vencido, pagado) pero no permite registrar el pago directamente desde ahí. Un botón "Registrar Pago" que cree la transacción e inserte en `OBLIGACION_TRANSACCION` completaría el ciclo de vida de una obligación.

**Notificaciones automáticas**  
Sería valioso que al iniciar sesión el sistema mostrara un resumen de obligaciones próximas a vencer (en los próximos 3 días) sin necesidad de ir al módulo de obligaciones. Una pequeña ventana emergente de alertas mejoraría la experiencia de uso diario.

**Presupuesto con desglose por mes**  
Actualmente un presupuesto cubre un período completo (ej: enero a marzo) y los detalles son montos mensuales fijos. Sería útil poder definir montos distintos por mes dentro del mismo presupuesto — por ejemplo, gastos más altos en diciembre.

**Exportación a Excel**  
La exportación a PDF está implementada pero los usuarios frecuentemente necesitan los datos en Excel para análisis adicional. Agregar exportación `.xlsx` usando una librería como EPPlus complementaría la funcionalidad de reportes.

**USUARIO ADMIN**
Varios compañeros, tenian un usuario Admin, y aunqe no pude ver exactamente el limite del poder de un ADMIN, seria util poder ver cosas como "Todos los Usuarios" o "Todos los presupuestos activos" de la base de datos, sin tener que irme a Oracle SqlDeveloper, y poner un query como "Select * from Usuarios", sino que la app traiga eso implementado para usuarios Administradores, y que la funcionalidad actual se mantenga tal como esta, pero para Usuarios Actuales

**Tema claro opcional**  
La interfaz tiene un tema oscuro fijo. Ofrecer un tema claro como opción en el perfil del usuario mejoraría la accesibilidad para diferentes preferencias y condiciones de iluminación.

---


