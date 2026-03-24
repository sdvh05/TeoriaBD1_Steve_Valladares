# 🖥️ Frontend — Sistema de Presupuesto Personal

Interfaz de escritorio: **C# Windows Forms (.NET Framework 4.7.2)**

---

## 📁 Estructura

```
TBD1_App/
└── Frontend/
    ├── FrmLogin.cs               ← Autenticación de usuarios
    ├── FrmRegistro.cs            ← Registro de nuevo usuario
    ├── FrmMenu.cs                ← Dashboard principal con resumen del mes
    ├── FrmTransacciones.cs       ← Lista y filtros de transacciones
    ├── FrmTransaccionDetalle.cs  ← Dialog nueva/editar transacción
    ├── FrmPresupuestos.cs        ← Lista + detalles de presupuestos
    │     (incluye FrmPresupuestoDetalle y FrmDetallePresupuesto)
    ├── FrmCategorias.cs          ← CRUD categorías y subcategorías
    │     (incluye FrmCategoriaDialog y FrmSubcategoriaDialog)
    ├── FrmObligaciones.cs        ← Lista con semáforo de urgencia
    │     (incluye FrmObligacionDialog)
    ├── FrmPerfil.cs              ← Ver y editar perfil del usuario
    └── FrmReportes.cs            ← 6 reportes con gráficas + exportar PDF
```

---

## 📦 Dependencias

| Paquete | Versión | Uso |
|---|---|---|
| `Oracle.ManagedDataAccess` | 23.x | Comunicación con Oracle DB |
| `iTextSharp` | 5.5.13.5 | Generación y exportación de PDFs |
| `Microsoft.VisualBasic` | — | `InputBox` para entrada de texto simple |
| `System.Drawing` | Nativo .NET | Gráficas dibujadas con GDI+ |
| `System.Windows.Forms` | Nativo .NET | Controles de interfaz |

> Las gráficas de los reportes (barras, pie, líneas, progreso) están dibujadas con **GDI+** nativo de .NET mediante el evento `Paint` — sin librerías externas de charting.

---

## 🎨 Sistema de Diseño

Todos los Forms comparten la misma paleta de colores definida como constantes:

```csharp
// Fondos
BG_DARK    = Color.FromArgb(15,  17,  23)   // fondo principal
BG_PANEL   = Color.FromArgb(22,  27,  34)   // headers y paneles
BG_CARD    = Color.FromArgb(30,  37,  48)   // tarjetas y grids
BG_INPUT   = Color.FromArgb(30,  37,  48)   // campos de texto

// Textos
TEXT_PRIMARY = Color.FromArgb(240, 246, 252) // texto principal
TEXT_MUTED   = Color.FromArgb(110, 118, 129) // texto secundario

// Acentos
ACCENT     = Color.FromArgb(56,  189, 248)   // azul — acción principal
COLOR_ING  = Color.FromArgb(63,  185, 120)   // verde — ingresos
COLOR_GAS  = Color.FromArgb(248,  81,  73)   // rojo — gastos
COLOR_AHO  = Color.FromArgb(251, 191,  36)   // amarillo — ahorros
```

---

## 🗺️ Flujo de navegación

```
FrmLogin
   ├── FrmRegistro (nuevo usuario)
   └── FrmMenu (login exitoso)
         ├── FrmPresupuestos
         │     ├── FrmPresupuestoDetalle (nuevo presupuesto)
         │     └── FrmDetallePresupuesto (agregar/editar detalle)
         ├── FrmTransacciones
         │     └── FrmTransaccionDetalle (nueva/editar transacción)
         ├── FrmCategorias
         │     ├── FrmCategoriaDialog (nueva/editar categoría)
         │     └── FrmSubcategoriaDialog (nueva/editar subcategoría)
         ├── FrmObligaciones
         │     └── FrmObligacionDialog (nueva/editar obligación)
         ├── FrmPerfil
         └── FrmReportes (6 tabs)
```

Todos los Forms secundarios se abren con `ShowDialog()` — bloqueando el Form padre hasta que se cierra. Al cerrar, el padre llama `CargarDatos()` para refrescar la información.

---

## 🖼️ Forms — descripción de cada uno

### FrmLogin
Autenticación del usuario. Llama `SP_AUTENTICAR_USUARIO` que valida correo y contraseña. Si falla 3 veces consecutivas muestra aviso. Botón para ir a registro.

### FrmRegistro
Formulario de nuevo usuario con validaciones en tiempo real: correo único, contraseña con confirmación, salario base. Llama `SP_INSERTAR_USUARIO`.

### FrmMenu
Dashboard principal. Muestra:
- Fecha actual y salario base del usuario
- 3 tarjetas de resumen: **Balance del mes** (ingresos − gastos), **Ahorros del mes**, **Próxima obligación** con días restantes
- 6 tarjetas de navegación a módulos (3 × 2)

El balance usa una query directa sin filtrar por presupuesto, sumando todas las transacciones del usuario en el mes actual.

### FrmTransacciones
Lista de transacciones con 4 filtros: presupuesto, año, mes ("Todos" itera los 12 meses), tipo. Muestra subtotales de ingresos/gastos/ahorros/balance actualizados al filtrar. Los tipos se muestran con colores: verde ingreso, rojo gasto, amarillo ahorro.

### FrmPresupuestos
Panel dividido: lista de presupuestos a la izquierda, detalle a la derecha. Los totales planeados (ingresos/gastos/ahorro) se calculan sumando los detalles en tiempo real — no se toman de campos del presupuesto que siempre son 0.

### FrmCategorias
CRUD completo con dos niveles: categorías y sus subcategorías. Al crear una categoría el trigger `TRG_SUBCAT_DEFECTO` crea automáticamente la subcategoría "General".

### FrmObligaciones
Lista con columna de semáforo visual por urgencia de vencimiento:
- 🟢 Verde: más de 7 días
- 🟡 Amarillo: 3–7 días
- 🟠 Naranja: menos de 3 días
- 🔴 Rojo: vencida

Usa `FN_DIAS_HASTA_VENCIMIENTO()` (función Oracle) para calcular los días restantes.

### FrmReportes
6 tabs con reportes analíticos. Cada tab tiene filtros, un panel de gráfica dibujada con GDI+ y una tabla con los datos. Botón **"Exportar PDF"** en el header genera un PDF con la gráfica capturada como imagen y la tabla de datos, usando iTextSharp.

---

## 📊 Gráficas GDI+

Las gráficas se dibujan en el evento `Paint` de un `Panel` usando `System.Drawing`:

| Reporte | Tipo de gráfica | Técnica |
|---|---|---|
| R1 Balance | Barras agrupadas | `FillRectangle` por grupo |
| R2 Gastos | Pie / dona | `FillPie` + `DrawPie` |
| R4 Tendencia | Líneas múltiples | `DrawLines` + `FillEllipse` |
| R6 Ahorro | Barras de progreso | Panels anidados con ancho proporcional |

```csharp
// Ejemplo: cómo se fuerza el redibujo al cargar datos
panel.Paint += (s, e) => {
    var g = e.Graphics;
    g.SmoothingMode = SmoothingMode.AntiAlias;
    // dibujar aquí con datos de la closure
};
panel.Invalidate(); // ← fuerza el evento Paint
```

---

## 🔒 Patrón de uso de DAOs desde los Forms

Los Forms nunca llaman Oracle directamente. El patrón es:

```csharp
public class FrmTransacciones : Form
{
    // 1. DAO declarado como campo privado
    private readonly TransaccionDAO _dao = new TransaccionDAO();

    // 2. Cargar datos al inicializar o refrescar
    private void CargarTransacciones()
    {
        var lista = _dao.ObtenerPorPresupuesto(idPre, anio, mes, tipo);
        dgv.Rows.Clear();
        foreach (var t in lista)
            dgv.Rows.Add(t.Fecha, t.Tipo, t.Monto, ...);
    }

    // 3. Al abrir dialog secundario, refrescar al volver
    private void BtnNueva_Click(object sender, EventArgs e)
    {
        var dlg = new FrmTransaccionDetalle(_usuario, _presupuesto, null);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            CargarTransacciones(); // ← refresca automáticamente
    }
}
```

---

## 📌 Notas de implementación

**Dialogs internos** — `FrmCategoriaDialog`, `FrmSubcategoriaDialog`, `FrmObligacionDialog` y `FrmDetallePresupuesto` están definidos en el mismo archivo `.cs` que su Form padre para mantener cohesión.

**ComboItem** — clase auxiliar usada en todos los ComboBox para almacenar un par `Valor` (ID de BD) + `Texto` (nombre visible):
```csharp
cbo.Items.Add(new ComboItem("PRE001", "Presupuesto 2026"));
string id = ((ComboItem)cbo.SelectedItem).Value; // extrae el ID
```

**FormBorderStyle.FixedDialog** — los dialogs secundarios usan este estilo: no se pueden redimensionar ni maximizar, solo cerrar.

**Exportar PDF** — el botón valida que el reporte fue generado (grid con filas), abre `SaveFileDialog` con nombre automático `Reporte_R1_Balance_20260321.pdf`, captura la gráfica con `DrawToBitmap` y genera el PDF con fondo oscuro usando iTextSharp.
