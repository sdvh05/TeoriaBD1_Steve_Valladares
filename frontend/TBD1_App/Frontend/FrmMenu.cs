using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TBD1_App.Backend;
using TBD1_App.Models;

namespace TBD1_App.Frontend
{
    public class FrmMenu : Form
    {
        // ── Colores ────────────────────────────────────────
        private readonly Color BG_DARK      = Color.FromArgb(15,  17,  23);
        private readonly Color BG_PANEL     = Color.FromArgb(22,  27,  34);
        private readonly Color BG_CARD      = Color.FromArgb(30,  37,  48);
        private readonly Color BG_CARD_HOV  = Color.FromArgb(38,  46,  60);
        private readonly Color ACCENT       = Color.FromArgb(56, 189, 248);
        private readonly Color TEXT_PRIMARY = Color.FromArgb(240, 246, 252);
        private readonly Color TEXT_MUTED   = Color.FromArgb(110, 118, 129);
        private readonly Color BORDER       = Color.FromArgb(48,  54,  61);
        private readonly Color COLOR_ING    = Color.FromArgb(63,  185, 120);  // verde
        private readonly Color COLOR_GAS    = Color.FromArgb(248,  81,  73);  // rojo
        private readonly Color COLOR_OBL    = Color.FromArgb(251, 191,  36);  // amarillo

        // ── DAOs ───────────────────────────────────────────
        private readonly ObligacionDAO _oblDAO = new ObligacionDAO();

        // ── Usuario en sesión ──────────────────────────────
        private readonly Usuario _usuario;

        // ── Controles de datos ─────────────────────────────
        private Label lblBalanceVal;
        private Label lblIngresosVal;
        private Label lblGastosVal;
        private Label lblObligacionVal;
        private Label lblSaludo;

        public FrmMenu(Usuario usuario)
        {
            _usuario = usuario;
            InicializarComponentes();
            CargarDatos();
        }

        private void InicializarComponentes()
        {
            this.Text            = "Presupuesto Personal — Menú Principal";
            this.Size            = new Size(1000, 680);
            this.MinimumSize     = new Size(1000, 680);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = BG_DARK;
            this.ForeColor       = TEXT_PRIMARY;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.Font            = new Font("Segoe UI", 9.5f);

            // ════════════════════════════════════════════════
            // HEADER
            // ════════════════════════════════════════════════
            var pnlHeader = new Panel
            {
                Size      = new Size(1000, 90),
                Location  = new Point(0, 0),
                BackColor = BG_PANEL
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawLine(pen, 0, 89, 1000, 89);
                // línea de acento izquierda
                using (var brush = new SolidBrush(ACCENT))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, 90);
            };
            this.Controls.Add(pnlHeader);

            // Título app
            var lblApp = new Label
            {
                Text      = "PRESUPUESTO PERSONAL",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize  = true,
                Location  = new Point(20, 18)
            };
            pnlHeader.Controls.Add(lblApp);

            // Saludo usuario
            lblSaludo = new Label
            {
                Text      = $"Bienvenido, {_usuario.PrimerNombre} {_usuario.PrimerApellido}",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(20, 50)
            };
            pnlHeader.Controls.Add(lblSaludo);

            // Fecha y salario — centrado en el header
            var lblFecha = new Label
            {
                Text      = $"{DateTime.Now:dddd, dd MMMM yyyy}  •  Salario base: L {_usuario.SalarioBase:N2}",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(400, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblFecha);

            // Botón cerrar sesión — derecha sin solapar
            var btnSalir = new Button
            {
                Text      = "Cerrar sesión",
                Font      = new Font("Segoe UI", 9f),
                Size      = new Size(110, 30),
                Location  = new Point(875, 55),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 248, 81, 73),
                ForeColor = Color.FromArgb(248, 81, 73),
                Cursor    = Cursors.Hand
            };
            btnSalir.FlatAppearance.BorderColor = Color.FromArgb(248, 81, 73);
            btnSalir.Click += (s, e) =>
            {
                var frm = new FrmLogin();
                frm.Show();
                this.Close();
            };
            pnlHeader.Controls.Add(btnSalir);

            // ════════════════════════════════════════════════
            // SECCIÓN DATOS RÁPIDOS
            // ════════════════════════════════════════════════
            var lblSecDatos = new Label
            {
                Text      = $"RESUMEN — {DateTime.Now:MMMM yyyy}".ToUpper(),
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(30, 112)
            };
            this.Controls.Add(lblSecDatos);

            // 3 tarjetas de datos
            CrearTarjetaDato(20,  135, "BALANCE DEL MES",   "Cargando...", ACCENT,    out lblBalanceVal);
            CrearTarjetaDato(350, 135, "AHORROS DEL MES",   "Cargando...", COLOR_ING, out lblIngresosVal);
            CrearTarjetaDato(680, 135, "PRÓX. OBLIGACIÓN",  "Cargando...", COLOR_OBL, out lblObligacionVal);

            // ════════════════════════════════════════════════
            // SECCIÓN MÓDULOS
            // ════════════════════════════════════════════════
            var lblSecModulos = new Label
            {
                Text      = "MÓDULOS",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(30, 250)
            };
            this.Controls.Add(lblSecModulos);

            // Tarjetas de navegación — fila 1 (3 módulos)
            CrearTarjetaModulo(30,  270, "💰", "Presupuestos",
                "Crear y gestionar presupuestos\nmensuales o por período",
                ACCENT, () => AbrirForm("presupuesto"));

            CrearTarjetaModulo(345, 270, "📊", "Transacciones",
                "Registrar ingresos, gastos\ny ahorros del mes",
                COLOR_ING, () => AbrirForm("transaccion"));

            CrearTarjetaModulo(660, 270, "🗂️", "Categorías",
                "Administrar categorías\ny subcategorías",
                Color.FromArgb(167, 139, 250), () => AbrirForm("categoria"));

            // Tarjetas de navegación — fila 2 (3 módulos)
            CrearTarjetaModulo(30,  460, "📋", "Obligaciones",
                "Gestionar pagos fijos\nmensuales recurrentes",
                COLOR_OBL, () => AbrirForm("obligacion"));

            CrearTarjetaModulo(345, 460, "👤", "Mi Perfil",
                "Ver y actualizar tu\ninformación personal",
                TEXT_MUTED, () => AbrirForm("perfil"));

            CrearTarjetaModulo(660, 460, "📈", "Reportes",
                "Análisis y gráficas\nde tu situación financiera",
                Color.FromArgb(167, 139, 250), () => AbrirForm("reportes"));
        }

        // ── Crea tarjeta de dato rápido ────────────────────
        private void CrearTarjetaDato(int x, int y, string titulo, string valorInicial, Color accentColor, out Label lblValor)
        {
            var pnl = new Panel
            {
                Size      = new Size(300, 90),
                Location  = new Point(x, y),
                BackColor = BG_CARD
            };
            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(BORDER, 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var brush = new SolidBrush(accentColor))
                    e.Graphics.FillRectangle(brush, 0, 0, 3, pnl.Height);
            };
            this.Controls.Add(pnl);

            var lblTit = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = TEXT_MUTED,
                AutoSize  = true,
                Location  = new Point(12, 12)
            };
            pnl.Controls.Add(lblTit);

            lblValor = new Label
            {
                Text      = valorInicial,
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize  = false,
                Size      = new Size(280, 44),
                Location  = new Point(12, 36),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnl.Controls.Add(lblValor);
        }

        // ── Crea tarjeta de módulo de navegación ───────────
        private void CrearTarjetaModulo(int x, int y, string icono, string titulo, string descripcion, Color accentColor, Action alClick)
        {
            var pnl = new Panel
            {
                Size      = new Size(295, 165),
                Location  = new Point(x, y),
                BackColor = BG_CARD,
                Cursor    = Cursors.Hand
            };

            // Borde y línea superior de color
            pnl.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(BORDER, 1))
                    g.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var brush = new SolidBrush(accentColor))
                    g.FillRectangle(brush, 0, 0, pnl.Width, 3);
            };

            // Hover effect
            pnl.MouseEnter += (s, e) => { pnl.BackColor = BG_CARD_HOV; pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { pnl.BackColor = BG_CARD;     pnl.Invalidate(); };
            pnl.Click      += (s, e) => alClick();

            this.Controls.Add(pnl);

            // Ícono
            var lblIcono = new Label
            {
                Text      = icono,
                Font      = new Font("Segoe UI Emoji", 28f),
                AutoSize  = true,
                Location  = new Point(15, 20),
                BackColor = Color.Transparent
            };
            lblIcono.Click += (s, e) => alClick();
            pnl.Controls.Add(lblIcono);

            // Título
            var lblTit = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = TEXT_PRIMARY,
                AutoSize  = true,
                Location  = new Point(15, 80),
                BackColor = Color.Transparent
            };
            lblTit.Click += (s, e) => alClick();
            pnl.Controls.Add(lblTit);

            // Descripción
            var lblDesc = new Label
            {
                Text      = descripcion,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = TEXT_MUTED,
                AutoSize  = false,
                Size      = new Size(185, 40),
                Location  = new Point(15, 112),
                BackColor = Color.Transparent
            };
            lblDesc.Click += (s, e) => alClick();
            pnl.Controls.Add(lblDesc);
        }

        // ── Carga datos desde Oracle ───────────────────────
        private void CargarDatos()
        {
            try
            {
                int anio = DateTime.Now.Year;
                int mes  = DateTime.Now.Month;

                // Query directa — suma todas las transacciones del mes sin filtrar por presupuesto
                var dt = OracleHelper.ExecuteQuery(@"
                    SELECT
                        NVL(SUM(CASE WHEN TIPO = 'ingreso' THEN MONTO ELSE 0 END), 0) AS INGRESOS,
                        NVL(SUM(CASE WHEN TIPO = 'gasto'   THEN MONTO ELSE 0 END), 0) AS GASTOS,
                        NVL(SUM(CASE WHEN TIPO = 'ahorro'  THEN MONTO ELSE 0 END), 0) AS AHORROS
                    FROM TRANSACCION
                    WHERE ID_USER = :u
                      AND ANIO    = :a
                      AND MES     = :m",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("u", _usuario.IdUser),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("a", anio),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("m", mes));

                decimal ingresos = Convert.ToDecimal(dt.Rows[0]["INGRESOS"]);
                decimal gastos   = Convert.ToDecimal(dt.Rows[0]["GASTOS"]);
                decimal ahorros  = Convert.ToDecimal(dt.Rows[0]["AHORROS"]);
                decimal balance  = ingresos - gastos;

                lblBalanceVal.Text      = $"L {balance:N2}";
                lblBalanceVal.ForeColor = balance >= 0 ? COLOR_ING : COLOR_GAS;
                lblIngresosVal.Text     = $"L {ahorros:N2}";

                // Próxima obligación — ordenar por días más cercanos
                var obligaciones = _oblDAO.ObtenerActivas(_usuario.IdUser);
                if (obligaciones.Count > 0)
                {
                    obligaciones.Sort((a, b) => a.DiasVencimiento.CompareTo(b.DiasVencimiento));
                    var prox = obligaciones[0];
                    string diasStr = prox.DiasVencimiento < 0  ? "¡Vencida!" :
                                     prox.DiasVencimiento == 0 ? "¡Hoy!" :
                                     prox.DiasVencimiento == 1 ? "¡Mañana!" :
                                     $"en {prox.DiasVencimiento} días";
                    lblObligacionVal.Text      = $"{prox.Nombre} — L {prox.Monto:N2}\nDía {prox.Dia}  •  {diasStr}";
                    lblObligacionVal.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
                    lblObligacionVal.ForeColor = prox.DiasVencimiento <= 3 ? COLOR_GAS :
                                                 prox.DiasVencimiento <= 7 ? Color.FromArgb(251, 146, 60) : COLOR_OBL;
                }
                else
                {
                    lblObligacionVal.Text      = "Sin obligaciones activas";
                    lblObligacionVal.ForeColor = COLOR_OBL;
                }
            }
            catch
            {
                lblBalanceVal.Text    = "L 0.00";
                lblIngresosVal.Text   = "L 0.00";
                lblObligacionVal.Text = "Sin datos";
            }
        }

        // ── Navegar a los Forms ────────────────────────────
        private void AbrirForm(string modulo)
        {
            switch (modulo)
            {
                case "transaccion":
                    new FrmTransacciones(_usuario).ShowDialog(this);
                    CargarDatos();
                    break;
                // TODO: descomentar cuando cada Form esté listo
                case "presupuesto": new FrmPresupuestos(_usuario).ShowDialog(this); CargarDatos(); break;
                case "categoria": new FrmCategorias(_usuario).ShowDialog(this); break;
                case "obligacion": new FrmObligaciones(_usuario).ShowDialog(this); CargarDatos(); break;
                case "perfil": new FrmPerfil(_usuario).ShowDialog(this); break;
                case "reportes": new FrmReportes(_usuario).ShowDialog(this); break;
                default:
                    MessageBox.Show($"Módulo '{modulo}' próximamente.",
                        "En construcción", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }
    }
}
